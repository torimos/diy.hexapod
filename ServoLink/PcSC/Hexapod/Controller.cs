using Contracts;
using Data;
using Drivers;
using IK;
using System;
using System.Threading;

namespace Hexapod
{
    public class Controller
    {
        IIKSolver me = new IKSolverEx();

        public void Run(HexModel model)
        {
            //todo: GPPlayer

            SingleLegControl(model);

            GateSequence(model);

            Balance(model);

            SolveIKLegs(model, me);
        }


        private void SolveIKLegs(HexModel model, IIKSolver ik)
        {
            XYZ bodyFKPos;
            IKLegResult legIK;

            for (byte leg = 0; leg < HexConfig.LegsCount / 2; leg++)
            {
                bodyFKPos = ik.BodyFK(leg,
                    -model.LegsPos[leg].x + model.BodyPos.x + model.GaitPos[leg].x - model.TotalTrans.x,
                    model.LegsPos[leg].z + model.BodyPos.z + model.GaitPos[leg].z - model.TotalTrans.z,
                    model.LegsPos[leg].y + model.BodyPos.y + model.GaitPos[leg].y - model.TotalTrans.y,
                    model.GaitRotY[leg],
                    model.BodyRot.x, model.BodyRot.z, model.BodyRot.y,
                    model.TotalBal.x, model.TotalBal.z, model.TotalBal.y);
                legIK = ik.LegIK(leg,
                    model.LegsPos[leg].x - model.BodyPos.x + bodyFKPos.x - (model.GaitPos[leg].x - model.TotalTrans.x),
                    model.LegsPos[leg].z + model.BodyPos.z - bodyFKPos.z + (model.GaitPos[leg].z - model.TotalTrans.z),
                    model.LegsPos[leg].y + model.BodyPos.y - bodyFKPos.y + (model.GaitPos[leg].y - model.TotalTrans.y));
                if (legIK.Solution != IKSolutionResultType.Error)
                {
                    model.LegsAngle[leg] = legIK.Result;
                }
            }
            for (byte leg = (byte)(HexConfig.LegsCount / 2); leg < HexConfig.LegsCount; leg++)
            {
                bodyFKPos = ik.BodyFK(leg,
                    model.LegsPos[leg].x - model.BodyPos.x + model.GaitPos[leg].x - model.TotalTrans.x,
                    model.LegsPos[leg].z + model.BodyPos.z + model.GaitPos[leg].z - model.TotalTrans.z,
                    model.LegsPos[leg].y + model.BodyPos.y + model.GaitPos[leg].y - model.TotalTrans.y,
                    model.GaitRotY[leg],
                    model.BodyRot.x, model.BodyRot.z, model.BodyRot.y,
                    model.TotalBal.x, model.TotalBal.z, model.TotalBal.y);
                legIK = ik.LegIK(leg,
                    model.LegsPos[leg].x + model.BodyPos.x - bodyFKPos.x + (model.GaitPos[leg].x - model.TotalTrans.x),
                    model.LegsPos[leg].z + model.BodyPos.z - bodyFKPos.z + (model.GaitPos[leg].z - model.TotalTrans.z),
                    model.LegsPos[leg].y + model.BodyPos.y - bodyFKPos.y + (model.GaitPos[leg].y - model.TotalTrans.y));
                if (legIK.Solution != IKSolutionResultType.Error)
                {
                    model.LegsAngle[leg] = legIK.Result;
                }
            }
        }
        private void GateSequence(HexModel model)
        {
            //Check if the Gait is in motion - If not if we are going to start a motion try to align our Gaitstep to start with a good foot
            // for the direction we are about to go...

            model.TravelRequest = (Math.Abs(model.TravelLength.x) > HexConfig.TravelDeadZone)
                   || (Math.Abs(model.TravelLength.z) > HexConfig.TravelDeadZone)
                   || (Math.Abs(model.TravelLength.y) > HexConfig.TravelDeadZone) || model.Walking || (model.ForceGaitStepCnt != 0);
            if (!model.TravelRequest)
            {
                //Clear values under the cTravelDeadZone
                model.TravelLength.x = 0;
                model.TravelLength.z = 0;
                model.TravelLength.y = 0;
                //Gait NOT in motion, return to home position
            }

            //Calculate Gait sequence for all legs
            for (var LegIndex = 0; LegIndex < HexConfig.LegsCount; LegIndex++)
            {
                Gait(model, LegIndex);
            }

            //Advance to the next step
            model.GaitStep++;
            if (model.GaitStep > model.gaitCur.StepsInGait)
                model.GaitStep = 1;

            // If we have a force count decrement it now... 
            if (model.ForceGaitStepCnt > 0)
                model.ForceGaitStepCnt--;
        }
        private void Gait(HexModel model, int GaitCurrentLegNr)
        {
            // Try to reduce the number of time we look at GaitLegnr and Gaitstep
            int LegStep = model.GaitStep - model.gaitCur.GaitLegNr[GaitCurrentLegNr];

            //Leg middle up position OK
            //Gait in motion	                                                                                  
            // For Lifted pos = 1, 3, 5
            if ((model.TravelRequest && ((model.gaitCur.NrLiftedPos & 1) > 0) && LegStep == 0) ||
                (!model.TravelRequest && LegStep == 0 && ((Math.Abs(model.GaitPos[GaitCurrentLegNr].x) > 2) || (Math.Abs(model.GaitPos[GaitCurrentLegNr].z) > 2) || (Math.Abs(model.GaitRotY[GaitCurrentLegNr]) > 2))))
            { //Up
                model.GaitPos[GaitCurrentLegNr].x = 0;
                model.GaitPos[GaitCurrentLegNr].y = -model.LegLiftHeight;
                model.GaitPos[GaitCurrentLegNr].z = 0;
                model.GaitRotY[GaitCurrentLegNr] = 0;
            }
            //Optional Half heigth Rear (2, 3, 5 lifted positions)
            else if (((model.gaitCur.NrLiftedPos == 2 && LegStep == 0) || (model.gaitCur.NrLiftedPos >= 3 && (LegStep == -1 || LegStep == (model.gaitCur.StepsInGait - 1)))) && model.TravelRequest)
            {
                model.GaitPos[GaitCurrentLegNr].x = -model.TravelLength.x / model.gaitCur.LiftDivFactor;
                model.GaitPos[GaitCurrentLegNr].y = -3 * model.LegLiftHeight / (3 + model.gaitCur.HalfLiftHeight);     //Easier to shift between div factor: /1 (3/3), /2 (3/6) and 3/4
                model.GaitPos[GaitCurrentLegNr].z = -model.TravelLength.z / model.gaitCur.LiftDivFactor;
                model.GaitRotY[GaitCurrentLegNr] = -model.TravelLength.y / model.gaitCur.LiftDivFactor;
            }
            // _A_	  
            // Optional Half heigth front (2, 3, 5 lifted positions)
            else if ((model.gaitCur.NrLiftedPos >= 2) && (LegStep == 1 || LegStep == -(model.gaitCur.StepsInGait - 1)) && model.TravelRequest)
            {
                model.GaitPos[GaitCurrentLegNr].x = model.TravelLength.x / model.gaitCur.LiftDivFactor;
                model.GaitPos[GaitCurrentLegNr].y = -3 * model.LegLiftHeight / (3 + model.gaitCur.HalfLiftHeight); // Easier to shift between div factor: /1 (3/3), /2 (3/6) and 3/4
                model.GaitPos[GaitCurrentLegNr].z = model.TravelLength.z / model.gaitCur.LiftDivFactor;
                model.GaitRotY[GaitCurrentLegNr] = model.TravelLength.y / model.gaitCur.LiftDivFactor;
            }

            //Optional Half heigth Rear 5 LiftedPos (5 lifted positions)
            else if (((model.gaitCur.NrLiftedPos == 5 && (LegStep == -2))) && model.TravelRequest)
            {
                model.GaitPos[GaitCurrentLegNr].x = -model.TravelLength.x / 2;
                model.GaitPos[GaitCurrentLegNr].y = -model.LegLiftHeight / 2;
                model.GaitPos[GaitCurrentLegNr].z = -model.TravelLength.z / 2;
                model.GaitRotY[GaitCurrentLegNr] = -model.TravelLength.y / 2;
            }

            //Optional Half heigth Front 5 LiftedPos (5 lifted positions)
            else if ((model.gaitCur.NrLiftedPos == 5) && (LegStep == 2 || LegStep == -(model.gaitCur.StepsInGait - 2)) && model.TravelRequest)
            {
                model.GaitPos[GaitCurrentLegNr].x = model.TravelLength.x / 2;
                model.GaitPos[GaitCurrentLegNr].y = -model.LegLiftHeight / 2;
                model.GaitPos[GaitCurrentLegNr].z = model.TravelLength.z / 2;
                model.GaitRotY[GaitCurrentLegNr] = model.TravelLength.y / 2;
            }
            //_B_
            //Leg front down position //bug here?  From _A_ to _B_ there should only be one gaitstep, not 2!
            //For example, where is the case of LegStep==0+2 executed when NRLiftedPos=3?
            else if ((LegStep == model.gaitCur.FrontDownPos || LegStep == -(model.gaitCur.StepsInGait - model.gaitCur.FrontDownPos)) && model.GaitPos[GaitCurrentLegNr].y < 0)
            {
                model.GaitPos[GaitCurrentLegNr].x = model.TravelLength.x / 2;
                model.GaitPos[GaitCurrentLegNr].z = model.TravelLength.z / 2;
                model.GaitRotY[GaitCurrentLegNr] = model.TravelLength.y / 2;
                model.GaitPos[GaitCurrentLegNr].y = 0;
            }
            //Move body forward      
            else
            {
                model.GaitPos[GaitCurrentLegNr].x = model.GaitPos[GaitCurrentLegNr].x - (model.TravelLength.x / model.gaitCur.TLDivFactor);
                model.GaitPos[GaitCurrentLegNr].y = 0;
                model.GaitPos[GaitCurrentLegNr].z = model.GaitPos[GaitCurrentLegNr].z - (model.TravelLength.z / model.gaitCur.TLDivFactor);
                model.GaitRotY[GaitCurrentLegNr] = model.GaitRotY[GaitCurrentLegNr] - (model.TravelLength.y / model.gaitCur.TLDivFactor);
            }
        }
        private void BalCalcOneLeg(HexModel model, double posX, double posZ, double posY, int BalLegNr)
        {
            //Calculating totals from center of the body to the feet
            double CPR_Z = HexConfig.OffsetZ[BalLegNr] + posZ;
            double CPR_X = HexConfig.OffsetX[BalLegNr] + posX;
            double CPR_Y = 15 + posY;        // using the value 150 to lower the centerpoint of rotation 'g_InControlState.BodyPos.y +

            model.TotalTrans.y += posY;
            model.TotalTrans.z += CPR_Z;
            model.TotalTrans.x += CPR_X;

            model.TotalBal.y += (Math.Atan2(CPR_Z, CPR_X) * 180) / Math.PI;
            model.TotalBal.z += ((Math.Atan2(CPR_Y, CPR_X) * 180) / Math.PI) - 90; //Rotate balance circle 90 deg
            model.TotalBal.x += ((Math.Atan2(CPR_Y, CPR_Z) * 180) / Math.PI) - 90; //Rotate balance circle 90 deg
        }
        private void Balance(HexModel model)
        {
            const int BalanceDivFactor = HexConfig.LegsCount;
            // Reset values used for calculation of balance
            model.TotalTrans.x = model.TotalTrans.y = model.TotalTrans.z = 0;
            model.TotalBal.x = model.TotalBal.y = model.TotalBal.z = 0;

            // Balance calculations
            if (model.BalanceMode)
            {
                // Balance Legs
                for (var LegIndex = 0; LegIndex < (HexConfig.LegsCount / 2); LegIndex++)
                {
                    BalCalcOneLeg(model, -model.LegsPos[LegIndex].x + model.GaitPos[LegIndex].x,
                        model.LegsPos[LegIndex].z + model.GaitPos[LegIndex].z,
                        (model.LegsPos[LegIndex].y - HexConfig.DefaultLegsPosY[LegIndex]) + model.GaitPos[LegIndex].y, LegIndex);
                }

                for (var LegIndex = (HexConfig.LegsCount / 2); LegIndex < HexConfig.LegsCount; LegIndex++)
                {
                    BalCalcOneLeg(model, model.LegsPos[LegIndex].x + model.GaitPos[LegIndex].x,
                        model.LegsPos[LegIndex].z + model.GaitPos[LegIndex].z,
                        (model.LegsPos[LegIndex].y - HexConfig.DefaultLegsPosY[LegIndex]) + model.GaitPos[LegIndex].y, LegIndex);
                }

                // BalanceBody
                model.TotalTrans.z = model.TotalTrans.z / BalanceDivFactor;
                model.TotalTrans.x = model.TotalTrans.x / BalanceDivFactor;
                model.TotalTrans.y = model.TotalTrans.y / BalanceDivFactor;

                if (model.TotalBal.y > 0)        //Rotate balance circle by +/- 180 deg
                    model.TotalBal.y -= 180;
                else
                    model.TotalBal.y += 100;

                if (model.TotalBal.z < -180)    //Compensate for extreme balance positions that causes overflow
                    model.TotalBal.z += 360;

                if (model.TotalBal.x < -180)    //Compensate for extreme balance positions that causes overflow
                    model.TotalBal.x += 360;

                //Balance rotation
                model.TotalBal.y = -model.TotalBal.y / BalanceDivFactor;
                model.TotalBal.x = -model.TotalBal.x / BalanceDivFactor;
                model.TotalBal.z = model.TotalBal.z / BalanceDivFactor;
            }
        }
        private void SingleLegControl(HexModel model)
        {
            if (model.ControlMode != HexModel.ControlModeType.SingleLeg) return;

            bool AllDown = (model.LegsPos[0].y == HexConfig.DefaultLegsPosY[0]) &&
                (model.LegsPos[1].y == HexConfig.DefaultLegsPosY[1]) &&
                (model.LegsPos[2].y == HexConfig.DefaultLegsPosY[2]) &&
                (model.LegsPos[3].y == HexConfig.DefaultLegsPosY[3]) &&
                (model.LegsPos[4].y == HexConfig.DefaultLegsPosY[4]) &&
                (model.LegsPos[5].y == HexConfig.DefaultLegsPosY[5]);

            if (model.SelectedLeg < HexConfig.LegsCount)
            {
                if (model.SelectedLeg != model.PrevSelectedLeg)
                {
                    if (AllDown)
                    {
                        //Lift leg a bit when it got selected
                        model.LegsPos[model.SelectedLeg].y = HexConfig.DefaultLegsPosY[model.SelectedLeg] - 30;
                        //Store current status
                        model.PrevSelectedLeg = model.SelectedLeg;
                    }
                    else
                    {
                        //Return prev leg back to the init position
                        model.LegsPos[model.PrevSelectedLeg].x = HexConfig.DefaultLegsPosX[model.PrevSelectedLeg];
                        model.LegsPos[model.PrevSelectedLeg].y = HexConfig.DefaultLegsPosY[model.PrevSelectedLeg];
                        model.LegsPos[model.PrevSelectedLeg].z = HexConfig.DefaultLegsPosZ[model.PrevSelectedLeg];
                    }
                }
                else if (!model.SingleLegHold)
                {
                    model.LegsPos[model.SelectedLeg].x = HexConfig.DefaultLegsPosX[model.SelectedLeg] + model.SingleLegPos.x;
                    model.LegsPos[model.SelectedLeg].y = HexConfig.DefaultLegsPosY[model.SelectedLeg] + model.SingleLegPos.y;
                    model.LegsPos[model.SelectedLeg].z = HexConfig.DefaultLegsPosZ[model.SelectedLeg] + model.SingleLegPos.z;
                }
            }
            else
            {
                //All legs to init position
                if (!AllDown)
                {
                    for (var LegIndex = 0; LegIndex < HexConfig.LegsCount; LegIndex++)
                    {
                        model.LegsPos[LegIndex].x = HexConfig.DefaultLegsPosX[LegIndex];
                        model.LegsPos[LegIndex].y = HexConfig.DefaultLegsPosY[LegIndex];
                        model.LegsPos[LegIndex].z = HexConfig.DefaultLegsPosZ[LegIndex];
                    }
                }
                if (model.PrevSelectedLeg != 0xFF)
                    model.PrevSelectedLeg = 0xFF;
            }
        }
        private void Calibrate(ServoDriver sd, DS6InputDriver id)
        {
            bool firstRun = true;
            int selLegIndex = 0;
            int selLegJoint = 0;
            int defAngleCoxa = 0;
            int defAngleFemur = -900;
            int defTibiaAngle = 0;
            int[][] legAngles = {
                new [] { defAngleCoxa, defAngleFemur, defTibiaAngle }, new[] { defAngleCoxa, defAngleFemur, defTibiaAngle }, new[] { defAngleCoxa, defAngleFemur, defTibiaAngle },
                new [] { defAngleCoxa, defAngleFemur, defTibiaAngle }, new[] { defAngleCoxa, defAngleFemur, defTibiaAngle }, new[] { defAngleCoxa, defAngleFemur, defTibiaAngle }
            };

            while (!id.Terminate)
            {
                var ks = id.Keyboard.GetCurrentState();
                var step = (ks.IsPressed(SlimDX.DirectInput.Key.LeftControl)) ? 10 : ((ks.IsPressed(SlimDX.DirectInput.Key.LeftShift)) ? 50 : 1);
                if (ks.IsPressed(SlimDX.DirectInput.Key.Escape)) break;
                else if (ks.IsPressed(SlimDX.DirectInput.Key.DownArrow))
                {
                    selLegIndex = (selLegIndex + 1) % 6;
                    sd.MoveAll(0);
                }
                else if (ks.IsPressed(SlimDX.DirectInput.Key.UpArrow))
                {
                    selLegIndex--; if (selLegIndex < 0) selLegIndex = 5;
                    sd.MoveAll(0);
                }
                else if (ks.IsPressed(SlimDX.DirectInput.Key.RightArrow)) selLegJoint = (selLegJoint + 1) % 3;
                else if (ks.IsPressed(SlimDX.DirectInput.Key.LeftArrow)) { selLegJoint--; if (selLegJoint < 0) selLegJoint = 2; }
                else if (ks.IsPressed(SlimDX.DirectInput.Key.A)) legAngles[selLegIndex][selLegJoint] += step;
                else if (ks.IsPressed(SlimDX.DirectInput.Key.Z)) legAngles[selLegIndex][selLegJoint] -= step;
                else if (ks.IsPressed(SlimDX.DirectInput.Key.S)) { if (selLegJoint == 0) ServoDriver.CoxaOffset[selLegIndex] += step; else if (selLegJoint == 1) ServoDriver.FemurOffset[selLegIndex] += step; else if (selLegJoint == 2) ServoDriver.TibiaOffset[selLegIndex] += step; }
                else if (ks.IsPressed(SlimDX.DirectInput.Key.X)) { if (selLegJoint == 0) ServoDriver.CoxaOffset[selLegIndex] -= step; else if (selLegJoint == 1) ServoDriver.FemurOffset[selLegIndex] -= step; else if (selLegJoint == 2) ServoDriver.TibiaOffset[selLegIndex] -= step; }

                if (ks.PressedKeys.Count > 0 || firstRun)
                {
                    firstRun = false;
                    sd.Move(selLegIndex * 3 + 2, (ushort)(1500 + (selLegIndex > 2 ? -legAngles[selLegIndex][0] : legAngles[selLegIndex][0]) + ServoDriver.CoxaOffset[selLegIndex]), 0);
                    sd.Move(selLegIndex * 3 + 1, (ushort)(1500 + (selLegIndex > 2 ? -legAngles[selLegIndex][1] : legAngles[selLegIndex][1]) + ServoDriver.FemurOffset[selLegIndex]), 0);
                    sd.Move(selLegIndex * 3 + 0, (ushort)(1500 + (selLegIndex > 2 ? -legAngles[selLegIndex][2] : legAngles[selLegIndex][2]) + ServoDriver.TibiaOffset[selLegIndex]), 0);
                    sd.Commit();

                    var isCoxa = selLegJoint == 0 ? "<" : " ";
                    var isFemur = selLegJoint == 1 ? "<" : " ";
                    var isTibia = selLegJoint == 2 ? "<" : " ";
                    Console.SetCursorPosition(0, 0);
                    for (var i = 0; i < 6; i++)
                    {
                        var selLeg = i == selLegIndex ? "<" : " ";
                        Console.WriteLine($"{i}{selLeg}: {legAngles[i][0],5} {ServoDriver.CoxaOffset[i],5}{isCoxa}   {legAngles[i][1],5} {ServoDriver.FemurOffset[i],5}{isFemur}   {legAngles[i][2],5} {ServoDriver.TibiaOffset[i],5}{isTibia}");
                    }
                    Thread.Sleep(100);
                }
            }
        }
        public void DebugOutput(HexModel model, IInputDriver inputDriver)
        {
            if (model.DebugOutput)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(model);
                inputDriver.DebugOutput();

                Thread.Sleep(100);
            }
        }
    }
}

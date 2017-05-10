using System.Threading;
using Unity.Configurator;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ServoCommander
{
    partial class Program
    {
        static void Main(string[] args)
        {
            new UnityRuntimeConfiguration().SetupContainer();
            var model = new HexModel(HexConfig.LegsCount);
            Setup(model);

            var me = new IKSolver();
            var sd = new ServoDriver();
            var id = new InputDriver();
            if (!sd.Init()) Console.WriteLine("Connection error!");
            sd.Reset();

            var sw = new Stopwatch();
            Console.SetWindowSize(80, 70);
            while (true)
            {
                sw.Restart();
                model.AdjustLegsPosition = false;
                var input = id.ProcessInput(model);
                if (input == null) break;

                if (model.AdjustLegsPosition)
                {
                    AdjustLegPositionsToBodyHeight(model);
                }
                SingleLegControl(model);

                GateSequence(model);

                Balance(model);

                SolveIKLegs(model, me);

                if (model.PowerOn)
                {
                    //Calculate Servo Move time
                    if ((Math.Abs(model.TravelLength.x) > HexConfig.TravelDeadZone)
                      || (Math.Abs(model.TravelLength.z) > HexConfig.TravelDeadZone)
                      || (Math.Abs(model.TravelLength.y * 2) > HexConfig.TravelDeadZone))
                    {
                        model.MoveTime = (ushort)(model.gaitCur.NomGaitSpeed + (model.InputTimeDelay * 2) + model.Speed);

                        //Add aditional delay when Balance mode is on
                        if (model.BalanceMode)
                            model.MoveTime = (ushort)(model.MoveTime + 100);
                    }
                    else //Movement speed excl. Walking
                        model.MoveTime = (ushort)(200 + model.Speed);

                    sd.Update(model.LegsAngle, model.MoveTime);

                    for (var LegIndex = 0; LegIndex < HexConfig.LegsCount; LegIndex++)
                    {
                        if (((Math.Abs(model.GaitPos[LegIndex].x) > HexConfig.GPlimit) || 
                            (Math.Abs(model.GaitPos[LegIndex].z) > HexConfig.GPlimit) || 
                            (Math.Abs(model.GaitPos[LegIndex].y) > HexConfig.GPlimit)) &&
                            model.ExtraCycle == 0)
                        {
                            //For making sure that we are using timed move until all legs are down
                            model.ExtraCycle = model.gaitCur.NrLiftedPos + 1;
                            break;
                        }
                    }
                    if (model.ExtraCycle > 0)
                    {
                        model.ExtraCycle--;
                        model.Walking = !(model.ExtraCycle == 0);
                        long timeToWait = (model.PrevMoveTime - sw.ElapsedMilliseconds);
                        sw.Restart();
                        do
                        {
                            DebugOutput(model);
                        }
                        while (sw.ElapsedMilliseconds < timeToWait);
                    }
                    
                    sd.Commit();
                }
                else
                {
                    if (model.PrevPowerOn)
                    {
                        model.MoveTime = 600;
                        sd.Update(model.LegsAngle, model.MoveTime);
                        sd.Commit();
                        Thread.Sleep(600);
                    }
                    else
                    {
                        sd.Reset();
                    }
                }

                DebugOutput(model);

                model.PrevControlMode = model.ControlMode;
                model.PrevMoveTime = model.MoveTime;
                model.PrevPowerOn = model.PowerOn;
            }
            sd.Reset();
            sd.Dispose();
            id.Release();
        }
        private static void Setup(HexModel model)
        {
            //Gait
            var gaits = new Dictionary<GaitType, PhoenixGait>();
            gaits.Add(GaitType.Ripple12, new PhoenixGait //Ripple 12
            {
                NomGaitSpeed = 70,
                StepsInGait = 12,
                NrLiftedPos = 3,
                FrontDownPos = 2,
                LiftDivFactor = 2,
                TLDivFactor = 8,
                HalfLiftHeight = 3,
                GaitLegNr = new byte[] { 7, 11, 3, 1, 5, 9 }
            });
            gaits.Add(GaitType.Tripod8, new PhoenixGait //Tripod 8
            {
                NomGaitSpeed = 70,
                StepsInGait = 8,
                NrLiftedPos = 3,
                FrontDownPos = 2,
                LiftDivFactor = 2,
                TLDivFactor = 4,
                HalfLiftHeight = 3,
                GaitLegNr = new byte[] { 1, 5, 1, 5, 1, 5 }
            });
            gaits.Add(GaitType.TripleTripod12, new PhoenixGait //Triple Tripod 12
            {
                NomGaitSpeed = 50,
                StepsInGait = 12,
                NrLiftedPos = 3,
                FrontDownPos = 2,
                LiftDivFactor = 2,
                TLDivFactor = 8,
                HalfLiftHeight = 3,
                GaitLegNr = new byte[] { 5, 10, 3, 11, 4, 9 }
            });
            gaits.Add(GaitType.TripleTripod16, new PhoenixGait //Triple Tripod 16 steps, use 5 lifted positions
            {
                NomGaitSpeed = 50,
                StepsInGait = 16,
                NrLiftedPos = 5,
                FrontDownPos = 3,
                LiftDivFactor = 4,
                TLDivFactor = 10,
                HalfLiftHeight = 1,
                GaitLegNr = new byte[] { 6, 13, 4, 14, 5, 12 }
            });
            gaits.Add(GaitType.Wave24, new PhoenixGait //Wave 24 steps
            {
                NomGaitSpeed = 70,
                StepsInGait = 24,
                NrLiftedPos = 3,
                FrontDownPos = 2,
                LiftDivFactor = 2,
                TLDivFactor = 20,
                HalfLiftHeight = 3,
                GaitLegNr = new byte[] { 13, 17, 21, 1, 5, 9 }
            });
            gaits.Add(GaitType.Tripod6, new PhoenixGait //Tripod 6 steps
            {
                NomGaitSpeed = 70,
                StepsInGait = 6,
                NrLiftedPos = 2,
                FrontDownPos = 1,
                LiftDivFactor = 2,
                TLDivFactor = 4,
                HalfLiftHeight = 1,
                GaitLegNr = new byte[] { 1, 4, 1, 4, 1, 4 }
            });
            model.GaitType = GaitType.Ripple12;
            model.BalanceMode = false;
            model.LegLiftHeight = HexConfig.LegLiftHeight;
            model.ForceGaitStepCnt = 0;    // added to try to adjust starting positions depending on height...
            model.GaitStep = 1;
            model.Gaits = gaits;
            model.gaitCur = model.Gaits[model.GaitType];

            for (var i = 0; i < 6; i++)
            {
                model.LegsPos[i] = new XYZ(HexConfig.DefaultLegsPosX[i], HexConfig.DefaultLegsPosY[i], HexConfig.DefaultLegsPosZ[i]);
            }

            model.PrevSelectedLeg = model.SelectedLeg = 0xFF; // No Leg selected
            model.Speed = 100;
            model.PowerOn = false;
            model.DebugOutput = true;
        }

        private static void SolveIKLegs(HexModel model, IKSolver ik)
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

        private static void DebugOutput(HexModel model)
        {
            if (model.DebugOutput)
            {
                if (model.PrevControlMode != model.ControlMode)
                {
                    Console.Clear();
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(model);
            }
        }


        private static void GateSequence(HexModel model)
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

        private static void Gait(HexModel model, int GaitCurrentLegNr)
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

        private static void BalCalcOneLeg(HexModel model, double posX, double posZ, double posY, int BalLegNr)
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

        private static void Balance(HexModel model)
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

        private static void AdjustLegPositionsToBodyHeight(HexModel model)
        {
            const double MIN_XZ_LEG_ADJUST = HexConfig.CoxaLength;
            const double MAX_XZ_LEG_ADJUST = HexConfig.CoxaLength + HexConfig.TibiaLength + HexConfig.FemurLength / 4;
            double[] hexIntXZ = { 111, 88, 86 };
            double[] hexMaxBodyY = { 20, 50, HexConfig.MaxBodyHeight };

            // Lets see which of our units we should use...
            // Note: We will also limit our body height here...
            model.BodyPos.y = Math.Min(model.BodyPos.y, HexConfig.MaxBodyHeight);
            double XZLength = hexIntXZ[2];
            int i;
            for (i = 0; i < 2; i++)
            {    // Don't need to look at last entry as we already init to assume this one...
                if (model.BodyPos.y <= hexMaxBodyY[i])
                {
                    XZLength = hexIntXZ[i];
                    break;
                }
            }
            if (i != model.LegInitIndex)
            {
                model.LegInitIndex = i;  // remember the current index...
                
                //now lets see what happens when we change the leg positions...
                if (XZLength > MAX_XZ_LEG_ADJUST)
                    XZLength = MAX_XZ_LEG_ADJUST;
                if (XZLength < MIN_XZ_LEG_ADJUST)
                    XZLength = MIN_XZ_LEG_ADJUST;


                // see if same length as when we came in
                if (XZLength == model.LegsXZLength)
                    return;

                model.LegsXZLength = XZLength;

                for (var legIndex = 0; legIndex < HexConfig.LegsCount; legIndex++)
                {
                    model.LegsPos[legIndex].x = Math.Cos(Math.PI * HexConfig.CoxaDefaultAngle[legIndex] / 180) * XZLength;  //Set start positions for each leg
                    model.LegsPos[legIndex].z = -Math.Sin(Math.PI * HexConfig.CoxaDefaultAngle[legIndex] / 180) * XZLength;
                }

                // Make sure we cycle through one gait to have the legs all move into their new locations...
                model.ForceGaitStepCnt = model.gaitCur.StepsInGait;
            }
        }

        private static void SingleLegControl(HexModel model)
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
                        model.LegsPos[model.SelectedLeg].y = HexConfig.DefaultLegsPosY[model.SelectedLeg] - 20;
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
    }
}

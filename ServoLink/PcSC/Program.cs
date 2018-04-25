using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Drivers;
using Hexapod;
using ServoCommander.Drivers;

namespace ServoCommander
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 70);

            var hex = new Controller();
            var sd = new ServoDriver(20);
            var id = new SerialInputDriver(); //new DS6InputDriver();
            sd.Init("COM3");
            sd.Reset();

            var model = new HexModel(HexConfig.LegsCount);
            InitModel(model);

            Task.Run(() =>
            {
                while (!id.Terminate)
                {
                    hex.DebugOutput(model, id);
                }
            });
            Task.Run(() =>
            {
                while (!id.Terminate)
                {
                    id.ProcessInput(model);
                }
            });

            var sw = new Stopwatch();
            while (!id.Terminate)
            {
                sw.Restart();
                // id.ProcessInput(model);

                hex.Run(model);

                if (model.PowerOn)
                {
                    //Calculate Servo Move time
                    if (model.ControlMode == HexModel.ControlModeType.SingleLeg)
                    {
                        model.MoveTime = HexConfig.SingleLegControlDelay;
                    }
                    else
                    {
                        if ((Math.Abs(model.TravelLength.x) > HexConfig.TravelDeadZone)
                          || (Math.Abs(model.TravelLength.z) > HexConfig.TravelDeadZone)
                          || (Math.Abs(model.TravelLength.y * 2) > HexConfig.TravelDeadZone))
                        {
                            model.MoveTime = (ushort)(model.gaitCur.NomGaitSpeed + (model.InputTimeDelay * 2) + model.Speed);

                            //Add aditional delay when Balance mode is on
                            if (model.BalanceMode)
                                model.MoveTime = (ushort)(model.MoveTime + HexConfig.BalancingDelay);
                        }
                        else //Movement speed excl. Walking
                            model.MoveTime = (ushort)(HexConfig.WalkingDelay + model.Speed);
                    }

                    sd.Update(model.LegsAngle, model.MoveTime);

                    for (var LegIndex = 0; LegIndex < HexConfig.LegsCount; LegIndex++)
                    {
                        if (((Math.Abs(model.GaitPos[LegIndex].x) > HexConfig.GPlimit) ||
                            (Math.Abs(model.GaitPos[LegIndex].z) > HexConfig.GPlimit) ||
                            (Math.Abs(model.GaitRotY[LegIndex]) > HexConfig.GPlimit)))
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

                model.PrevControlMode = model.ControlMode;
                model.PrevMoveTime = model.MoveTime;
                model.PrevPowerOn = model.PowerOn;
            }


            sd.Reset();
            sd.Dispose();
            id.Release();
            Console.WriteLine(sd.ReadLastResult());
        }

        private static void InitModel(HexModel model)
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
            model.GaitType = GaitType.Tripod6;
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
            model.Speed = 150;
            model.PowerOn = false;
            model.DebugOutput = true;
        }
    }
}

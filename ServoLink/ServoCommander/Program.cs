using System.Threading;
using Unity.Configurator;
using System;
using System.Threading.Tasks;

namespace ServoCommander
{
    partial class Program
    {
        static void Main(string[] args)
        {
            new UnityRuntimeConfiguration().SetupContainer();
            var model = new HexModel(6);
            model.LegsPos[0] = new XYZ(96, 60, 96);
            model.LegsPos[1] = new XYZ(111, 60, 0);
            model.LegsPos[2] = new XYZ(96, 60, -96);
            model.LegsPos[3] = new XYZ(96, 60, 96);
            model.LegsPos[4] = new XYZ(111, 60, 0);
            model.LegsPos[5] = new XYZ(96, 60, -96);

            var me = new IKMath();
            var sd = new ServoDriver();
            var id = new InputDriver();
            if (!sd.Init()) Console.WriteLine("Connection error!");
            sd.Reset();
            sd.Reset();
            sd.Reset();

            //bool runUpdates = true;
            //Task.Run(() =>
            //{
            //    while (runUpdates)
            //    {
            //        sd.Update(model.LegsAngle, model.MoveTime);
            //        Thread.Sleep(20);
            //    }
            //});
            Console.SetWindowSize(80, 50);
            while (true)
            {
                var input = id.ProcessInput(model);
                if (input == false) break;

                XYZ bodyFKPos;
                IKLegResult legIK;
                for (byte leg = 0; leg < model.LegsCount / 2; leg++)
                {
                    bodyFKPos = me.BodyFK(leg,
                        model.LegsPos[leg].x + model.BodyPos.x + model.GatePos[leg].x - model.TotalTrans.x,
                        model.LegsPos[leg].z + model.BodyPos.z + model.GatePos[leg].z - model.TotalTrans.z,
                        model.LegsPos[leg].y + model.BodyPos.y + model.GatePos[leg].y - model.TotalTrans.y,
                        model.GateRotY[leg],
                        model.BodyRot.x, model.BodyRot.z, model.BodyRot.y,
                        model.TotalBal.x, model.TotalBal.z, model.TotalBal.y);
                    legIK = me.LegIK(leg,
                        model.LegsPos[leg].x - model.BodyPos.x + bodyFKPos.x - (model.GatePos[leg].x - model.TotalTrans.x),
                        model.LegsPos[leg].z + model.BodyPos.z - bodyFKPos.z + (model.GatePos[leg].z - model.TotalTrans.z),
                        model.LegsPos[leg].y + model.BodyPos.y - bodyFKPos.y + (model.GatePos[leg].y - model.TotalTrans.y));
                    if (legIK.Solution != IKSolutionResultType.Error)
                    {
                        model.LegsAngle[leg] = legIK.Result;
                    }
                }
                for (byte leg = (byte)(model.LegsCount / 2); leg < model.LegsCount; leg++)
                {
                    bodyFKPos = me.BodyFK(leg,
                        model.LegsPos[leg].x - model.BodyPos.x + model.GatePos[leg].x - model.TotalTrans.x,
                        model.LegsPos[leg].z + model.BodyPos.z + model.GatePos[leg].z - model.TotalTrans.z,
                        model.LegsPos[leg].y + model.BodyPos.y + model.GatePos[leg].y - model.TotalTrans.y,
                        model.GateRotY[leg],
                        model.BodyRot.x, model.BodyRot.z, model.BodyRot.y,
                        model.TotalBal.x, model.TotalBal.z, model.TotalBal.y);
                    legIK = me.LegIK(leg,
                        model.LegsPos[leg].x + model.BodyPos.x - bodyFKPos.x + (model.GatePos[leg].x - model.TotalTrans.x),
                        model.LegsPos[leg].z + model.BodyPos.z - bodyFKPos.z + (model.GatePos[leg].z - model.TotalTrans.z),
                        model.LegsPos[leg].y + model.BodyPos.y - bodyFKPos.y + (model.GatePos[leg].y - model.TotalTrans.y));
                    if (legIK.Solution != IKSolutionResultType.Error)
                    {
                        model.LegsAngle[leg] = legIK.Result;
                    }
                }

                //if (model.SelectedLeg != 0xFF)
                //{
                //    int leg = model.SelectedLeg % 6;
                //    model.LegsAngle[leg].Coxa = IKMathConfig.CoxaAngleInv[leg] ? -model.BodyRot.x : model.BodyRot.x;
                //    model.LegsAngle[leg].Femur = IKMathConfig.FemurAngleInv[leg] ? -model.BodyRot.z : model.BodyRot.z;
                //    model.LegsAngle[leg].Tibia = IKMathConfig.TibiaAngleInv[leg] ? -model.BodyRot.y : model.BodyRot.y;
                //}


                if (model.PowerOn)
                {
                    model.MoveTime = 200;
                    sd.Update(model.LegsAngle, model.MoveTime);
                }
                else
                {
                    if (model.PrevPowerOn)
                    {
                        model.MoveTime = 600;
                        sd.Update(model.LegsAngle, model.MoveTime);
                        Thread.Sleep(600);
                    }
                    else
                    {
                        sd.Reset();
                    }
                }

                

                model.PrevMoveTime = model.MoveTime;
                model.PrevPowerOn = model.PowerOn;

                Console.SetCursorPosition(0, 0);
                Console.WriteLine(model);
            }
            //runUpdates = false;
            //Thread.Sleep(150);
            sd.Reset();
            sd.Dispose();
        }
    }
}

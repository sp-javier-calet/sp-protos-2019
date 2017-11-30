using NUnit.Framework;
using NSubstitute;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.ServerSync;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Components
{
    [TestFixture]
    class LifecycleControllerTests
    {
        const float dtTest = 0.33f;

        ISetupComponent _setupComp1;
        ISetupComponent _setupComp2;
        IUpdateComponent _updateComp1;
        IUpdateComponent _updateComp2;
        ICleanupComponent _cleanupComp1;
        ICleanupComponent _cleanupComp2;
        IStopComponent _stopComp1;
        IStopComponent _stopComp2;
        IBattleErrorHandler _errorHandlerComp;
        LifecycleController _battleController;

        [SetUp]
        public void SetUp()
        {
            _setupComp1 = Substitute.For<ISetupComponent>();
            _setupComp2 = Substitute.For<ISetupComponent>();
            _updateComp1 = Substitute.For<IUpdateComponent>();
            _updateComp2 = Substitute.For<IUpdateComponent>();
            _cleanupComp1 = Substitute.For<ICleanupComponent>();
            _cleanupComp2 = Substitute.For<ICleanupComponent>();
            _stopComp1 = Substitute.For<IStopComponent>();
            _stopComp2 = Substitute.For<IStopComponent>();
            _errorHandlerComp = Substitute.For<IBattleErrorHandler>();
            _battleController = new LifecycleController(
                Substitute.For<IUpdateScheduler>()
            );
            _battleController.Start();
        }

        void CleanReceivedCalls()
        {
            _setupComp1.ClearReceivedCalls();
            _setupComp2.ClearReceivedCalls();
            _updateComp1.ClearReceivedCalls();
            _updateComp2.ClearReceivedCalls();
            _cleanupComp1.ClearReceivedCalls();
            _cleanupComp2.ClearReceivedCalls();
        }

        [Test]
        public void StepOrder()
        {
            _setupComp1.Update(Arg.Any<float>()).Returns(SetupStepState.Success);
            _battleController.RegisterSetupComponent(_setupComp1);
            _battleController.RegisterUpdateComponent(_updateComp1);
            _battleController.RegisterCleanupComponent(_cleanupComp1);

            _battleController.Update(dtTest); // setup step

            _setupComp1.Received(1).Start();
            _setupComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            _cleanupComp1.DidNotReceive().Cleanup();
            CleanReceivedCalls();

            _battleController.Update(dtTest); // start step
            _battleController.Update(dtTest);

            _setupComp1.DidNotReceive().Start();
            _setupComp1.DidNotReceive().Update(Arg.Any<float>());
            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _cleanupComp1.DidNotReceive().Cleanup();
            CleanReceivedCalls();

            _battleController.Dispose();
            _battleController.Update(dtTest);

            _setupComp1.DidNotReceive().Start();
            _setupComp1.DidNotReceive().Update(Arg.Any<float>());
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            _cleanupComp1.Received(1).Cleanup();
        }

        [Test]
        public void NoSetupSucess()
        {
            _battleController.RegisterUpdateComponent(_updateComp1);

            _battleController.Update(dtTest);//Run empty Setup
            _battleController.Update(dtTest);//Run start
            _battleController.Update(dtTest);

            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
        }

        [Test]
        public void MultiSetupSuccess()
        {
            _setupComp1.Update(Arg.Any<float>()).Returns(SetupStepState.Success);
            _setupComp2.Update(Arg.Any<float>()).Returns(SetupStepState.Success);
            _battleController.RegisterSetupComponent(_setupComp1);
            _battleController.RegisterSetupComponent(_setupComp2);
            _battleController.RegisterUpdateComponent(_updateComp1);

            _battleController.Update(dtTest);

            _setupComp1.Received(1).Start();
            _setupComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _setupComp2.DidNotReceive().Start();
            _setupComp2.DidNotReceive().Update(Arg.Any<float>());
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            CleanReceivedCalls();

            _battleController.Update(dtTest);

            _setupComp1.DidNotReceive().Start();
            _setupComp1.DidNotReceive().Update(Arg.Any<float>());
            _setupComp2.Received(1).Start();
            _setupComp2.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            CleanReceivedCalls();

            _battleController.Update(dtTest);

            _setupComp1.DidNotReceive().Start();
            _setupComp1.DidNotReceive().Update(Arg.Any<float>());
            _setupComp2.DidNotReceive().Start();
            _setupComp2.DidNotReceive().Update(Arg.Any<float>());

            _battleController.Update(dtTest);

            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
        }

        [Test]
        public void MultiSetupFailure()
        {
            _setupComp1.When(setupComp => setupComp.Update(Arg.Any<float>())).Do(setupComp => ((IBattleErrorHandler)_battleController).OnError(new BattleError()));
            _setupComp2.Update(Arg.Any<float>()).Returns(SetupStepState.Success);
            _battleController.RegisterSetupComponent(_setupComp1);
            _battleController.RegisterSetupComponent(_setupComp2);
            _battleController.RegisterUpdateComponent(_updateComp1);
            _battleController.RegisterCleanupComponent(_cleanupComp1);

            _battleController.Update(dtTest);

            _setupComp1.Received(1).Start();
            _setupComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _setupComp2.DidNotReceive().Start();
            _setupComp2.DidNotReceive().Update(Arg.Any<float>());
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            _cleanupComp1.Received(1).Cleanup();
        }

        [Test]
        public void MultiUpdate()
        {
            _battleController.RegisterUpdateComponent(_updateComp1);
            _battleController.RegisterUpdateComponent(_updateComp2);

            _battleController.Update(dtTest);//Run empty Setup
            _battleController.Update(dtTest);//Run start
            _battleController.Update(dtTest);

            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _updateComp2.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
        }

        [Test]
        public void MultiCleanup()
        {
            _battleController.RegisterCleanupComponent(_cleanupComp1);
            _battleController.RegisterCleanupComponent(_cleanupComp2);

            _battleController.Dispose();

            _cleanupComp1.Received(1).Cleanup();
            _cleanupComp2.Received(1).Cleanup();
        }


        [Test]
        public void StopSuccess()
        {
            _stopComp1.When(s => s.Stop()).Do(s => ((IStopListener)_battleController).OnStopped(true));
            _battleController.RegisterStopComponent(_stopComp1);
            _battleController.RegisterCleanupComponent(_cleanupComp1);

            _battleController.Stop();

            _stopComp1.Received(1).Stop();
        }

        [Test]
        public void StopFailure()
        {
            _stopComp1.When(s => s.Stop()).Do(s => ((IStopListener)_battleController).OnStopped(true));
            _stopComp2.When(s => s.Stop()).Do(s => ((IStopListener)_battleController).OnStopped(false));
            _battleController.RegisterStopComponent(_stopComp1);
            _battleController.RegisterStopComponent(_stopComp2);
            _battleController.RegisterCleanupComponent(_cleanupComp1);

            _battleController.Stop();

            _stopComp1.Received(1).Stop();
            _stopComp2.Received(1).Stop();
            _cleanupComp1.Received(0).Cleanup();
        }

        [Test]
        public void ErrorHandlerSetup()
        {
            _setupComp1.When(setupComp => setupComp.Update(Arg.Any<float>())).Do(setupComp => ((IBattleErrorHandler)_battleController).OnError(new BattleError()));
            _battleController.RegisterSetupComponent(_setupComp1);
            _battleController.RegisterUpdateComponent(_updateComp1);
            _battleController.RegisterErrorHandlerComponent(_errorHandlerComp);

            _battleController.Update(dtTest);
            _battleController.Update(dtTest);//Should not update components after error

            _setupComp1.Received(1).Start();
            _setupComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            _errorHandlerComp.Received(1).OnError(Arg.Any<BattleError>());
        }

        [Test]
        public void ErrorHandlerUpdate()
        {
            _updateComp1.When(updateComp => updateComp.Update(Arg.Any<float>())).Do(updateComp => ((IBattleErrorHandler)_battleController).OnError(new BattleError()));
            _battleController.RegisterUpdateComponent(_updateComp1);
            _battleController.RegisterErrorHandlerComponent(_errorHandlerComp);

            _battleController.Update(dtTest);//Run empty Setup
            _battleController.Update(dtTest);
            _battleController.Update(dtTest);//Should not update components after error

            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _errorHandlerComp.Received(1).OnError(Arg.Any<BattleError>());
        }

        public static bool NearlyEqual(float a, float b) {
            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float diff = Math.Abs(a - b);

            if (a == b)
            {
                return true;
            }
            else
            {
                return diff / (absA + absB) < float.MinValue;
            }
        }
    }
}

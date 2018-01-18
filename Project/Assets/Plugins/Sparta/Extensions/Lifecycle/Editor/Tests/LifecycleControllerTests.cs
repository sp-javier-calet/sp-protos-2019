using NUnit.Framework;
using NSubstitute;
using System;
using SocialPoint.Base;

namespace SocialPoint.Lifecycle
{
    [TestFixture]
    class LifecycleControllerTests
    {
        const float dtTest = 0.33f;

        public interface ITestSetupCleanupComponent : ISetupComponent, ICleanupComponent
        {
        }

        ISetupComponent _setupComp1;
        ISetupComponent _setupComp2;
        IUpdateComponent _updateComp1;
        IUpdateComponent _updateComp2;
        ICleanupComponent _cleanupComp1;
        ICleanupComponent _cleanupComp2;
        ICancelComponent _cancelComp1;
        ICancelComponent _cancelComp2;
        IErrorHandler _errorHandler;
        LifecycleController _controller;

        [SetUp]
        public void SetUp()
        {
            _setupComp1 = Substitute.For<ISetupComponent>();
            _setupComp2 = Substitute.For<ISetupComponent>();
            _updateComp1 = Substitute.For<IUpdateComponent>();
            _updateComp2 = Substitute.For<IUpdateComponent>();
            _cleanupComp1 = Substitute.For<ICleanupComponent>();
            _cleanupComp2 = Substitute.For<ICleanupComponent>();
            _cancelComp1 = Substitute.For<ICancelComponent>();
            _cancelComp2 = Substitute.For<ICancelComponent>();
            _errorHandler = Substitute.For<IErrorHandler>();
            _controller = new LifecycleController();
            _controller.Start();
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
            _setupComp1.Finished.Returns(true);
            _controller.RegisterSetupComponent(_setupComp1);
            _controller.RegisterUpdateComponent(_updateComp1);
            _controller.RegisterCleanupComponent(_cleanupComp1);

            _controller.Update(dtTest); // setup step

            _setupComp1.Received(1).Start();
            _setupComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            _cleanupComp1.DidNotReceive().Cleanup();
            CleanReceivedCalls();

            _controller.Update(dtTest); // start step
            _controller.Update(dtTest);

            _setupComp1.DidNotReceive().Start();
            _setupComp1.DidNotReceive().Update(Arg.Any<float>());
            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _cleanupComp1.DidNotReceive().Cleanup();
            CleanReceivedCalls();

            _controller.Dispose();
            _controller.Update(dtTest);

            _setupComp1.DidNotReceive().Start();
            _setupComp1.DidNotReceive().Update(Arg.Any<float>());
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            _cleanupComp1.Received(1).Cleanup();
        }

        [Test]
        public void NoSetupSucess()
        {
            _controller.RegisterUpdateComponent(_updateComp1);

            _controller.Update(dtTest);//Run empty Setup
            _controller.Update(dtTest);//Run start
            _controller.Update(dtTest);

            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
        }

        [Test]
        public void MultiSetupSuccess()
        {
            _setupComp1.Finished.Returns(true);
            _setupComp2.Finished.Returns(true);
            _controller.RegisterSetupComponent(_setupComp1);
            _controller.RegisterSetupComponent(_setupComp2);
            _controller.RegisterUpdateComponent(_updateComp1);

            _controller.Update(dtTest);

            _setupComp1.Received(1).Start();
            _setupComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _setupComp2.DidNotReceive().Start();
            _setupComp2.DidNotReceive().Update(Arg.Any<float>());
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            CleanReceivedCalls();

            _controller.Update(dtTest);

            _setupComp1.DidNotReceive().Start();
            _setupComp1.DidNotReceive().Update(Arg.Any<float>());
            _setupComp2.Received(1).Start();
            _setupComp2.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            CleanReceivedCalls();

            _controller.Update(dtTest);

            _setupComp1.DidNotReceive().Start();
            _setupComp1.DidNotReceive().Update(Arg.Any<float>());
            _setupComp2.DidNotReceive().Start();
            _setupComp2.DidNotReceive().Update(Arg.Any<float>());

            _controller.Update(dtTest);

            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
        }

        [Test]
        public void MultiSetupFailure()
        {
            _setupComp1.When(setupComp => setupComp.Update(Arg.Any<float>())).Do(setupComp => ((IErrorHandler)_controller).OnError(new Error()));
            _setupComp2.Finished.Returns(true);
            _controller.RegisterSetupComponent(_setupComp1);
            _controller.RegisterSetupComponent(_setupComp2);
            _controller.RegisterUpdateComponent(_updateComp1);
            _controller.RegisterCleanupComponent(_cleanupComp1);

            _controller.Update(dtTest);

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
            _controller.RegisterUpdateComponent(_updateComp1);
            _controller.RegisterUpdateComponent(_updateComp2);

            _controller.Update(dtTest);//Run empty Setup
            _controller.Update(dtTest);//Run start
            _controller.Update(dtTest);

            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _updateComp2.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
        }

        [Test]
        public void MultiCleanup()
        {
            _controller.RegisterCleanupComponent(_cleanupComp1);
            _controller.RegisterCleanupComponent(_cleanupComp2);

            _controller.Dispose();

            _cleanupComp1.Received(1).Cleanup();
            _cleanupComp2.Received(1).Cleanup();
        }


        [Test]
        public void CancelSuccess()
        {
            _controller.RegisterCancelComponent(_cancelComp1);
            _controller.RegisterCleanupComponent(_cleanupComp1);

            _controller.Cancel();

            _cancelComp1.Received(1).Cancel();
            _cleanupComp1.DidNotReceive().Cleanup();

            _cancelComp1.Listener.OnCancelled(true);
            _cleanupComp1.Received(1).Cleanup();
        }

        [Test]
        public void CancelSuccessDisposeDisabled()
        {
            _controller.DisposeAfterCancel = false;
            _controller.RegisterCancelComponent(_cancelComp1);
            _controller.RegisterCleanupComponent(_cleanupComp1);

            _controller.Cancel();
            _cancelComp1.Listener.OnCancelled(true);
            _cleanupComp1.DidNotReceive().Cleanup();
        }

        [Test]
        public void CancelFailure()
        {
            _cancelComp1.When(s => s.Cancel()).Do(s => _cancelComp1.Listener.OnCancelled(true));
            _cancelComp2.When(s => s.Cancel()).Do(s => _cancelComp1.Listener.OnCancelled(false));
            _controller.RegisterCancelComponent(_cancelComp1);
            _controller.RegisterCancelComponent(_cancelComp2);
            _controller.RegisterCleanupComponent(_cleanupComp1);

            _controller.Cancel();

            _cancelComp1.Received(1).Cancel();
            _cancelComp2.Received(1).Cancel();
            _cleanupComp1.DidNotReceive().Cleanup();
        }

        [Test]
        public void ErrorHandlerSetup()
        {
            _setupComp1.When(setupComp => setupComp.Update(Arg.Any<float>())).Do(setupComp => ((IErrorHandler)_controller).OnError(new Error()));
            _controller.RegisterSetupComponent(_setupComp1);
            _controller.RegisterUpdateComponent(_updateComp1);
            _controller.RegisterErrorHandler(_errorHandler);

            _controller.Update(dtTest);
            _controller.Update(dtTest);//Should not update components after error

            _setupComp1.Received(1).Start();
            _setupComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _updateComp1.DidNotReceive().Update(Arg.Any<float>());
            _errorHandler.Received(1).OnError(Arg.Any<Error>());
        }

        [Test]
        public void ErrorHandlerUpdate()
        {
            _updateComp1.When(updateComp => updateComp.Update(Arg.Any<float>())).Do(updateComp => ((IErrorHandler)_controller).OnError(new Error()));
            _controller.RegisterUpdateComponent(_updateComp1);
            _controller.RegisterErrorHandler(_errorHandler);

            _controller.Update(dtTest);//Run empty Setup
            _controller.Update(dtTest);
            _controller.Update(dtTest);//Should not update components after error

            _updateComp1.Received(1).Update(Arg.Is<float>(dt => NearlyEqual(dt, dtTest)));
            _errorHandler.Received(1).OnError(Arg.Any<Error>());
        }

        public static bool NearlyEqual(float a, float b) {
            return UnityEngine.Mathf.Approximately(a, b);
        }

        [Test]
        public void SetupCleanupComponent()
        {
            var comp1 = Substitute.For<ITestSetupCleanupComponent>();
            var comp2 = Substitute.For<ICleanSetupComponent>();
            var comp3 = Substitute.For<ITestSetupCleanupComponent>();

            _controller.RegisterComponent(comp1);
            _controller.RegisterComponent(comp2);
            _controller.RegisterComponent(comp3);

            _controller.Update(dtTest);

            _controller.Cancel();

            comp1.Received(1).Start();
            comp1.Received(1).Update(dtTest);
            comp1.Received(1).Cleanup();
            comp2.DidNotReceive().Start();
            comp2.DidNotReceive().Update(Arg.Any<float>());
            comp2.DidNotReceive().Cleanup();
            comp3.DidNotReceive().Start();
            comp3.DidNotReceive().Update(Arg.Any<float>());
            comp3.Received(1).Cleanup();
        }
    }
}

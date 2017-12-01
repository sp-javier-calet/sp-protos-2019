using NUnit.Framework;
using NSubstitute;

namespace SocialPoint.Lifecycle
{
    [TestFixture]
    class ActionProcessorTests
    {
        class TestAction
        {
        }

        class TestOtherAction
        {
        }

        enum TestActionResult
        {
            Error1,
            Error2
        };

        class State
        {
        }

        TestAction _action;
        TestOtherAction _otherAction;
        IActionValidator<TestAction> _validator1;
        IActionValidator<TestAction> _validator2;
        IActionValidator<TestOtherAction> _otherValidator;
        IActionHandler<TestAction> _successHandler1;
        IActionHandler<TestAction> _successHandler2;
        IActionHandler<TestAction> _failureHandler1;
        IActionHandler<TestAction> _failureHandler2;
        IStateActionHandler<object, TestAction> _stateHandler; 

        IActionHandler<TestOtherAction> _otherSuccessHandler;
        ActionProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _action = new TestAction();
            _otherAction = new TestOtherAction();
            _validator1 = Substitute.For<IActionValidator<TestAction>>();
            _validator2 = Substitute.For<IActionValidator<TestAction>>();
            _otherValidator = Substitute.For<IActionValidator<TestOtherAction>>();
            _successHandler1 = Substitute.For<IActionHandler<TestAction>>();
            _successHandler2 = Substitute.For<IActionHandler<TestAction>>();
            _failureHandler1 = Substitute.For<IActionHandler<TestAction>>();
            _failureHandler2 = Substitute.For<IActionHandler<TestAction>>();
            _otherSuccessHandler = Substitute.For<IActionHandler<TestOtherAction>>();
            _processor = new ActionProcessor();
        }

        [Test]
        public void BasicHandlers()
        {
            TestAction receivedAction = null;
            _processor.RegisterHandler((TestAction a) => receivedAction = a);
            _processor.RegisterHandler(_successHandler1);
            _processor.Process(_action);
            _successHandler1.Received(1).Handle(_action);
            Assert.AreEqual(_action, receivedAction);

            _processor.UnregisterHandler(_successHandler1);
            _successHandler1.ClearReceivedCalls();
            _processor.Process(_action);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestAction>());
        }

        [Test]
        public void BasicStateHandlers()
        {
            TestAction receivedAction = null;
            object receivedState = null;
            _processor.RegisterStateHandler((object s, TestAction a) => { receivedAction = a; receivedState = s; });
            _processor.RegisterHandler(_successHandler1);
            var state = new State();
            _processor.Process(state, _action);
            _successHandler1.Received(1).Handle(_action);
            Assert.AreEqual(_action, receivedAction);
            Assert.AreEqual(state, receivedState);
        }

        [Test]
        public void NoValidatorAutoSuccess()
        {
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_action);
            _successHandler1.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
            _failureHandler1.DidNotReceive().Handle(Arg.Any<TestAction>());
        }

        [Test]
        public void ValidatorSuccess()
        {
            _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_action);
            _successHandler1.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
            _failureHandler1.DidNotReceive().Handle(Arg.Any<TestAction>());
        }

        [Test]
        public void ValidatorFailure()
        {
            _validator1.Validate(Arg.Any<TestAction>()).Returns(false);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_action);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestAction>());
            _failureHandler1.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
        }

        [Test]
        public void ResultValidator()
        {
            var resultValidator = Substitute.For<IActionValidator<TestAction, TestActionResult>>();
            var failureResultHandler = Substitute.For<IResultActionHandler<TestAction, TestActionResult>>();

            TestActionResult result;
            resultValidator.Validate(Arg.Any<TestAction>(), out result)
                .Returns(x => {
                    x[1] = TestActionResult.Error2;
                    return false;
                });

            _processor.RegisterValidator(resultValidator);
            _processor.RegisterFailureHandler(failureResultHandler);
            _processor.Process(_action);
            failureResultHandler.Received(1).Handle(_action, TestActionResult.Error2);
        }

        [Test]
        public void MultiValidatorSuccess()
        {
            _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
            _validator2.Validate(Arg.Any<TestAction>()).Returns(true);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterValidator(_validator2);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_action);
            _successHandler1.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
            _failureHandler1.DidNotReceive().Handle(Arg.Any<TestAction>());
        }

        [Test]
        public void MultiValidatorFailure()
        {
            _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
            _validator2.Validate(Arg.Any<TestAction>()).Returns(false);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterValidator(_validator2);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_action);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestAction>());
            _failureHandler1.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
        }

        [Test]
        public void MultiHandlerSuccess()
        {
            _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterSuccessHandler(_successHandler2);
            _processor.RegisterFailureHandler(_failureHandler1);
            _processor.RegisterFailureHandler(_failureHandler2);

            _processor.Process(_action);
            _successHandler1.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
            _successHandler2.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
            _failureHandler1.DidNotReceive().Handle(Arg.Any<TestAction>());
            _failureHandler2.DidNotReceive().Handle(Arg.Any<TestAction>());
        }

        [Test]
        public void MultiHandlerFailure()
        {
            _validator1.Validate(Arg.Any<TestAction>()).Returns(false);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterSuccessHandler(_successHandler2);
            _processor.RegisterFailureHandler(_failureHandler1);
            _processor.RegisterFailureHandler(_failureHandler2);

            _processor.Process(_action);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestAction>());
            _successHandler2.DidNotReceive().Handle(Arg.Any<TestAction>());
            _failureHandler1.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
            _failureHandler2.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
        }

        [Test]
        public void MultiActionSelection()
        {
            _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
            _otherValidator.Validate(Arg.Any<TestOtherAction>()).Returns(true);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterValidator(_otherValidator);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterSuccessHandler(_otherSuccessHandler);

            _processor.Process(_action);
            _validator1.Received(1).Validate(Arg.Is<TestAction>(processingAction => processingAction == _action));
            _successHandler1.Received(1).Handle(Arg.Is<TestAction>(processingAction => processingAction == _action));
            _otherValidator.DidNotReceive().Validate(Arg.Any<TestOtherAction>());
            _otherSuccessHandler.DidNotReceive().Handle(Arg.Any<TestOtherAction>());

            _validator1.ClearReceivedCalls();
            _successHandler1.ClearReceivedCalls();
            _otherValidator.ClearReceivedCalls();
            _otherSuccessHandler.ClearReceivedCalls();

            _processor.Process(_otherAction);
            _validator1.DidNotReceive().Validate(Arg.Any<TestAction>());
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestAction>());
            _otherValidator.Received(1).Validate(Arg.Is<TestOtherAction>(processingAction => processingAction == _otherAction));
            _otherSuccessHandler.Received(1).Handle(Arg.Is<TestOtherAction>(processingAction => processingAction == _otherAction));
        }
    }
}

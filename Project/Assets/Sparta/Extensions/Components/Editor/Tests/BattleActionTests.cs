using NUnit.Framework;
using NSubstitute;

[TestFixture]
class BattleActionTests
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

    TestAction _action;
    TestOtherAction _otherAction;
    IBattleActionValidator<TestAction> _validator1;
    IBattleActionValidator<TestAction> _validator2;
    IBattleActionValidator<TestOtherAction> _otherValidator;
    IBattleActionHandler<TestAction> _successHandler1;
    IBattleActionHandler<TestAction> _successHandler2;
    IBattleActionHandler<TestAction> _failureHandler1;
    IBattleActionHandler<TestAction> _failureHandler2;

    IBattleActionHandler<TestOtherAction> _otherSuccessHandler;
    BattleActionDispatcher _dispatcher;

    [SetUp]
    public void SetUp()
    {
        _action = new TestAction();
        _otherAction = new TestOtherAction();
        _validator1 = Substitute.For<IBattleActionValidator<TestAction>>();
        _validator2 = Substitute.For<IBattleActionValidator<TestAction>>();
        _otherValidator = Substitute.For<IBattleActionValidator<TestOtherAction>>();
        _successHandler1 = Substitute.For<IBattleActionHandler<TestAction>>();
        _successHandler2 = Substitute.For<IBattleActionHandler<TestAction>>();
        _failureHandler1 = Substitute.For<IBattleActionHandler<TestAction>>();
        _failureHandler2 = Substitute.For<IBattleActionHandler<TestAction>>();
        _otherSuccessHandler = Substitute.For<IBattleActionHandler<TestOtherAction>>();
        _dispatcher = new BattleActionDispatcher();
    }

    [Test]
    public void NoValidatorAutoSuccess()
    {
        _dispatcher.RegisterSuccessHandler(_successHandler1);
        _dispatcher.RegisterFailureHandler(_failureHandler1);

        _dispatcher.ProcessAction(_action);
        _successHandler1.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
        _failureHandler1.DidNotReceive().Apply(Arg.Any<TestAction>());
    }

    [Test]
    public void ValidatorSuccess()
    {
        _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
        _dispatcher.RegisterValidator(_validator1);
        _dispatcher.RegisterSuccessHandler(_successHandler1);
        _dispatcher.RegisterFailureHandler(_failureHandler1);

        _dispatcher.ProcessAction(_action);
        _successHandler1.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
        _failureHandler1.DidNotReceive().Apply(Arg.Any<TestAction>());
    }

    [Test]
    public void ValidatorFailure()
    {
        _validator1.Validate(Arg.Any<TestAction>()).Returns(false);
        _dispatcher.RegisterValidator(_validator1);
        _dispatcher.RegisterSuccessHandler(_successHandler1);
        _dispatcher.RegisterFailureHandler(_failureHandler1);

        _dispatcher.ProcessAction(_action);
        _successHandler1.DidNotReceive().Apply(Arg.Any<TestAction>());
        _failureHandler1.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
    }

    [Test]
    public void ResultValidator()
    {
        var resultValidator = Substitute.For<IBattleActionValidator<TestAction, TestActionResult>>();
        var failureResultHandler = Substitute.For<IBattleActionHandler<TestAction, TestActionResult>>();

        TestActionResult result;
        resultValidator.Validate(Arg.Any<TestAction>(), out result)
            .Returns(x => {
                x[1] = TestActionResult.Error2;
                return false;
            });

        _dispatcher.RegisterValidator(resultValidator);
        _dispatcher.RegisterFailureHandler(failureResultHandler);
        _dispatcher.ProcessAction(_action);
        failureResultHandler.Received(1).Apply(_action, TestActionResult.Error2);
    }

    [Test]
    public void MultiValidatorSuccess()
    {
        _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
        _validator2.Validate(Arg.Any<TestAction>()).Returns(true);
        _dispatcher.RegisterValidator(_validator1);
        _dispatcher.RegisterValidator(_validator2);
        _dispatcher.RegisterSuccessHandler(_successHandler1);
        _dispatcher.RegisterFailureHandler(_failureHandler1);

        _dispatcher.ProcessAction(_action);
        _successHandler1.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
        _failureHandler1.DidNotReceive().Apply(Arg.Any<TestAction>());
    }

    [Test]
    public void MultiValidatorFailure()
    {
        _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
        _validator2.Validate(Arg.Any<TestAction>()).Returns(false);
        _dispatcher.RegisterValidator(_validator1);
        _dispatcher.RegisterValidator(_validator2);
        _dispatcher.RegisterSuccessHandler(_successHandler1);
        _dispatcher.RegisterFailureHandler(_failureHandler1);

        _dispatcher.ProcessAction(_action);
        _successHandler1.DidNotReceive().Apply(Arg.Any<TestAction>());
        _failureHandler1.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
    }

    [Test]
    public void MultiHandlerSuccess()
    {
        _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
        _dispatcher.RegisterValidator(_validator1);
        _dispatcher.RegisterSuccessHandler(_successHandler1);
        _dispatcher.RegisterSuccessHandler(_successHandler2);
        _dispatcher.RegisterFailureHandler(_failureHandler1);
        _dispatcher.RegisterFailureHandler(_failureHandler2);

        _dispatcher.ProcessAction(_action);
        _successHandler1.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
        _successHandler2.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
        _failureHandler1.DidNotReceive().Apply(Arg.Any<TestAction>());
        _failureHandler2.DidNotReceive().Apply(Arg.Any<TestAction>());
    }

    [Test]
    public void MultiHandlerFailure()
    {
        _validator1.Validate(Arg.Any<TestAction>()).Returns(false);
        _dispatcher.RegisterValidator(_validator1);
        _dispatcher.RegisterSuccessHandler(_successHandler1);
        _dispatcher.RegisterSuccessHandler(_successHandler2);
        _dispatcher.RegisterFailureHandler(_failureHandler1);
        _dispatcher.RegisterFailureHandler(_failureHandler2);

        _dispatcher.ProcessAction(_action);
        _successHandler1.DidNotReceive().Apply(Arg.Any<TestAction>());
        _successHandler2.DidNotReceive().Apply(Arg.Any<TestAction>());
        _failureHandler1.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
        _failureHandler2.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
    }

    [Test]
    public void MultiActionSelection()
    {
        _validator1.Validate(Arg.Any<TestAction>()).Returns(true);
        _otherValidator.Validate(Arg.Any<TestOtherAction>()).Returns(true);
        _dispatcher.RegisterValidator(_validator1);
        _dispatcher.RegisterValidator(_otherValidator);
        _dispatcher.RegisterSuccessHandler(_successHandler1);
        _dispatcher.RegisterSuccessHandler(_otherSuccessHandler);

        _dispatcher.ProcessAction(_action);
        _validator1.Received(1).Validate(Arg.Is<TestAction>(processingAction => processingAction == _action));
        _successHandler1.Received(1).Apply(Arg.Is<TestAction>(processingAction => processingAction == _action));
        _otherValidator.DidNotReceive().Validate(Arg.Any<TestOtherAction>());
        _otherSuccessHandler.DidNotReceive().Apply(Arg.Any<TestOtherAction>());

        _validator1.ClearReceivedCalls();
        _successHandler1.ClearReceivedCalls();
        _otherValidator.ClearReceivedCalls();
        _otherSuccessHandler.ClearReceivedCalls();

        _dispatcher.ProcessAction(_otherAction);
        _validator1.DidNotReceive().Validate(Arg.Any<TestAction>());
        _successHandler1.DidNotReceive().Apply(Arg.Any<TestAction>());
        _otherValidator.Received(1).Validate(Arg.Is<TestOtherAction>(processingAction => processingAction == _otherAction));
        _otherSuccessHandler.Received(1).Apply(Arg.Is<TestOtherAction>(processingAction => processingAction == _otherAction));
    }
}

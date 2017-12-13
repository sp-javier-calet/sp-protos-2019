using NUnit.Framework;
using NSubstitute;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Lifecycle
{
    [TestFixture]
    class EventProcessorTests
    {
        class TestEvent
        {
        }

        class TestOtherEvent
        {
        }

        class TestDerivedEvent : TestEvent
        {
        }

        enum TestResult
        {
            Error1,
            Error2
        };

        class State
        {
        }

        TestEvent _event;
        TestOtherEvent _otherEvent;
        IEventValidator<TestEvent> _validator1;
        IEventValidator<TestEvent> _validator2;
        IEventValidator<TestOtherEvent> _otherValidator;
        IEventHandler<TestEvent> _successHandler1;
        IEventHandler<TestEvent> _successHandler2;
        IEventHandler<TestEvent> _failureHandler1;
        IEventHandler<TestEvent> _failureHandler2;
        IStateEventHandler<object, TestEvent> _stateHandler; 

        IEventHandler<TestOtherEvent> _otherSuccessHandler;
        EventProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _event = new TestEvent();
            _otherEvent = new TestOtherEvent();
            _validator1 = Substitute.For<IEventValidator<TestEvent>>();
            _validator2 = Substitute.For<IEventValidator<TestEvent>>();
            _otherValidator = Substitute.For<IEventValidator<TestOtherEvent>>();
            _successHandler1 = Substitute.For<IEventHandler<TestEvent>>();
            _successHandler2 = Substitute.For<IEventHandler<TestEvent>>();
            _failureHandler1 = Substitute.For<IEventHandler<TestEvent>>();
            _failureHandler2 = Substitute.For<IEventHandler<TestEvent>>();
            _otherSuccessHandler = Substitute.For<IEventHandler<TestOtherEvent>>();
            _processor = new EventProcessor();
        }

        [Test]
        public void BasicHandlers()
        {
            TestEvent receivedEvent = null;
            _processor.RegisterHandler((TestEvent a) => receivedEvent = a);
            _processor.RegisterHandler(_successHandler1);
            _processor.Process(_event);
            _successHandler1.Received(1).Handle(_event);
            Assert.AreEqual(_event, receivedEvent);

            _processor.UnregisterHandler(_successHandler1);
            _successHandler1.ClearReceivedCalls();
            _processor.Process(_event);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
        }

        [Test]
        public void DerivedHandlers()
        {
            var ev = new TestDerivedEvent();
            TestDerivedEvent receivedEvent = null;
            _processor.RegisterHandler((TestDerivedEvent a) => receivedEvent = a);
            _processor.RegisterHandler(_successHandler1);
            _processor.Process(ev);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
            Assert.AreEqual(ev, receivedEvent);

            _processor.DerivedEventSupport = true;
            _processor.Process(ev);
            _successHandler1.Received(1).Handle(ev);
        }

        [Test]
        public void BasicStateHandlers()
        {
            TestEvent receivedEvent = null;
            object receivedState = null;
            _processor.RegisterStateHandler((object s, TestEvent a) => { receivedEvent = a; receivedState = s; });
            _processor.RegisterHandler(_successHandler1);
            var state = new State();
            _processor.Process(state, _event);
            _successHandler1.Received(1).Handle(_event);
            Assert.AreEqual(_event, receivedEvent);
            Assert.AreEqual(state, receivedState);
        }

        [Test]
        public void NoValidatorAutoSuccess()
        {
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_event);
            _successHandler1.Received(1).Handle(_event);
            _failureHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
        }

        [Test]
        public void ValidatorSuccess()
        {
            _validator1.Validate(Arg.Any<TestEvent>()).Returns(true);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_event);
            _successHandler1.Received(1).Handle(_event);
            _failureHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
        }

        [Test]
        public void ValidatorFailure()
        {
            _validator1.Validate(Arg.Any<TestEvent>()).Returns(false);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_event);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
            _failureHandler1.Received(1).Handle(_event);
        }

        [Test]
        public void ResultValidator()
        {
            var resultValidator = Substitute.For<IEventValidator<TestEvent, TestResult>>();
            var failureResultHandler = Substitute.For<IResultEventHandler<TestEvent, TestResult>>();

            TestResult result;
            resultValidator.Validate(Arg.Any<TestEvent>(), out result)
                .Returns(x => {
                    x[1] = TestResult.Error2;
                    return false;
                });

            _processor.RegisterValidator(resultValidator);
            _processor.RegisterFailureHandler(failureResultHandler);
            _processor.Process(_event);
            failureResultHandler.Received(1).Handle(_event, TestResult.Error2);
        }

        [Test]
        public void MultiValidatorSuccess()
        {
            _validator1.Validate(Arg.Any<TestEvent>()).Returns(true);
            _validator2.Validate(Arg.Any<TestEvent>()).Returns(true);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterValidator(_validator2);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_event);
            _successHandler1.Received(1).Handle(_event);
            _failureHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
        }

        [Test]
        public void MultiValidatorFailure()
        {
            _validator1.Validate(Arg.Any<TestEvent>()).Returns(true);
            _validator2.Validate(Arg.Any<TestEvent>()).Returns(false);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterValidator(_validator2);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterFailureHandler(_failureHandler1);

            _processor.Process(_event);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
            _failureHandler1.Received(1).Handle(_event);
        }

        [Test]
        public void MultiHandlerSuccess()
        {
            _validator1.Validate(Arg.Any<TestEvent>()).Returns(true);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterSuccessHandler(_successHandler2);
            _processor.RegisterFailureHandler(_failureHandler1);
            _processor.RegisterFailureHandler(_failureHandler2);

            _processor.Process(_event);
            _successHandler1.Received(1).Handle(_event);
            _successHandler2.Received(1).Handle(_event);
            _failureHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
            _failureHandler2.DidNotReceive().Handle(Arg.Any<TestEvent>());
        }

        [Test]
        public void MultiHandlerFailure()
        {
            _validator1.Validate(Arg.Any<TestEvent>()).Returns(false);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterSuccessHandler(_successHandler2);
            _processor.RegisterFailureHandler(_failureHandler1);
            _processor.RegisterFailureHandler(_failureHandler2);

            _processor.Process(_event);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
            _successHandler2.DidNotReceive().Handle(Arg.Any<TestEvent>());
            _failureHandler1.Received(1).Handle(_event);
            _failureHandler2.Received(1).Handle(_event);
        }

        [Test]
        public void MultiSelection()
        {
            _validator1.Validate(Arg.Any<TestEvent>()).Returns(true);
            _otherValidator.Validate(Arg.Any<TestOtherEvent>()).Returns(true);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterValidator(_otherValidator);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterSuccessHandler(_otherSuccessHandler);

            _processor.Process(_event);
            _validator1.Received(1).Validate(_event);
            _successHandler1.Received(1).Handle(_event);
            _otherValidator.DidNotReceive().Validate(Arg.Any<TestOtherEvent>());
            _otherSuccessHandler.DidNotReceive().Handle(Arg.Any<TestOtherEvent>());

            _validator1.ClearReceivedCalls();
            _successHandler1.ClearReceivedCalls();
            _otherValidator.ClearReceivedCalls();
            _otherSuccessHandler.ClearReceivedCalls();

            _processor.Process(_otherEvent);
            _validator1.DidNotReceive().Validate(Arg.Any<TestEvent>());
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
            _otherValidator.Received(1).Validate(Arg.Is<TestOtherEvent>(ev => ev == _otherEvent));
            _otherSuccessHandler.Received(1).Handle(Arg.Is<TestOtherEvent>(ev => ev == _otherEvent));
        }

        [Test]
        public void RecursiveProcess()
        {
            _processor.DerivedEventSupport = true;
            _processor.RegisterSuccessHandler((TestOtherEvent e) => {
                _processor.RegisterValidator(_validator1);
                _processor.RegisterSuccessHandler(_successHandler2);
                _processor.RegisterSuccessHandler(_otherSuccessHandler);
                _processor.Process(_event);
            });
            _processor.RegisterSuccessHandler(_successHandler1);
            _validator1.Validate(_event).Returns(true);

            // validators & handlers are added but don't trigger until next process
            _processor.Process(_otherEvent);
            _successHandler1.Received(1).Handle(_event);
            _validator1.DidNotReceive().Validate(Arg.Any<TestEvent>());
            _successHandler2.DidNotReceive().Handle(Arg.Any<TestEvent>());
            _otherSuccessHandler.DidNotReceive().Handle(Arg.Any<TestOtherEvent>());

            _processor.Process(_event);
            _validator1.Received(1).Validate(_event);
            _successHandler2.Received(1).Handle(_event);

            _processor.Process(_otherEvent);
            _otherSuccessHandler.Received(1).Handle(_otherEvent);
        }

        [Test]
        public void AggregateException()
        {
            _processor.RegisterHandler((TestEvent e) => { throw new Exception("aaa"); });
            _processor.RegisterHandler(_successHandler1);
            _processor.RegisterHandler((TestEvent e) => { _processor.RegisterHandler(_successHandler2); throw new InvalidOperationException("aaa"); });

            var exceptionCount = 0;
            try
            {
                _processor.Process(_event);
            }
            catch(AggregateException e)
            {
                exceptionCount = e.Exceptions.Count;
            }
            _successHandler1.Received(1).Handle(_event);
            Assert.AreEqual(2, exceptionCount);

            try
            {
                _processor.Process(_event);
            }
            catch(AggregateException e)
            {
            }
            _successHandler2.Received(1).Handle(_event);
        }

        [Test]
        public void DoubleRegister()
        {
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterSuccessHandler(_successHandler1);
            _processor.RegisterValidator(_validator1);
            _processor.RegisterValidator(_validator1);

            _processor.UnregisterValidator(_validator1);
            _processor.Process(_event);
            _successHandler1.Received(1).Handle(_event);
            _successHandler1.ClearReceivedCalls();

            _processor.UnregisterHandler(_successHandler1);
            _processor.Process(_event);
            _successHandler1.DidNotReceive().Handle(Arg.Any<TestEvent>());
        }
    }
}

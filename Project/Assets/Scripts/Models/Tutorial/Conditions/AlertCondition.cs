//-----------------------------------------------------------------------
// AlertCondition.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using SocialPoint.Alert;
using SocialPoint.Dependency;

namespace SocialPoint.Tutorial
{
    [Serializable]
    public class AlertCondition : ICondition
    {
        public string AlertMessage;

        [NonSerialized] bool _completed;
        [NonSerialized] bool _shown;

        IAlertView _alertPrototype;

        public void OnStartEvaluating()
        {
        }

        public bool Completed => _completed;

        public AlertCondition()
        {
            AlertMessage = "Alert message";
        }

        public void Update(float elapsed)
        {
            if(_shown || _completed)
            {
                return;
            }

            if(_alertPrototype == null)
            {
                _alertPrototype = Services.Instance.Resolve<IAlertView>();
                if(_alertPrototype == null)
                {
                    throw new InvalidOperationException("Could not find alert view");
                }
            }

            var alertView = (IAlertView)_alertPrototype.Clone();
            alertView.Title = "Tutorial Step";
            alertView.Message = AlertMessage;
            alertView.Input = false;
            alertView.Buttons = new []{ "Ok" };
            alertView.Show(result => { _completed = true; });

            _shown = true;
        }
    }
}

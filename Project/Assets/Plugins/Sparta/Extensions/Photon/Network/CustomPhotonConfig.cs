using System;

[Serializable]
public class CustomPhotonConfig
{
    bool _isSavedOriginalConfig = false;

    const int DefaultUpdateInterval = 100000;
    const int DefaultUpdateIntervalOnSerialize = 100000;
    const int DefaultMaximumTransferUnit = 1500;
    const int DefaultSentCountAllowance = 10; // Allow for big lags
    const int DefaultQuickResendAttempts = 0; // SpeedUp from second repeat on. This avoid resending repeats too fast

    public bool Enabled = false;

    int _originalUpdateInterval;
    public int UpdateInterval = DefaultUpdateInterval;

    int _originalUpdateIntervalOnSerialize;
    public int UpdateIntervalOnSerialize = DefaultUpdateIntervalOnSerialize;

    int _originalMaximumTransferUnit;
    public int MaximumTransferUnit = DefaultMaximumTransferUnit;

    int _originalSentCountAllowance;
    public int SentCountAllowance = DefaultSentCountAllowance;

    int _originalQuickResendAttempts = DefaultQuickResendAttempts;
    public int QuickResendAttempts = DefaultQuickResendAttempts;

    bool _pendingOutgoingCommands = false;

    void SaveOriginalPhotonSettings()
    {
        _originalUpdateInterval = PhotonNetwork.photonMono.updateInterval;
        _originalUpdateIntervalOnSerialize = PhotonNetwork.photonMono.updateIntervalOnSerialize;

        _originalSentCountAllowance = PhotonNetwork.MaxResendsBeforeDisconnect;
        _originalQuickResendAttempts = PhotonNetwork.QuickResends;

        _originalMaximumTransferUnit = PhotonNetwork.networkingPeer.MaximumTransferUnit;

        _isSavedOriginalConfig = true;
    }

    public void SetConfigBeforeConnection()
    {
        if(!Enabled)
        {
            return;
        }
        
        if(!_isSavedOriginalConfig)
        {
            SaveOriginalPhotonSettings();
        }

        PhotonNetwork.networkingPeer.MaximumTransferUnit = MaximumTransferUnit;
    }

    public void SetConfigOnJoinedRoom()
    {
        if(!Enabled)
        {
            return;
        }

        if(!_isSavedOriginalConfig)
        {
            SaveOriginalPhotonSettings();
        }

        if(PhotonNetwork.connected)
        {
            PhotonNetwork.photonMono.updateInterval = UpdateInterval;
            PhotonNetwork.photonMono.updateIntervalOnSerialize = UpdateIntervalOnSerialize;

            PhotonNetwork.MaxResendsBeforeDisconnect = SentCountAllowance;
            PhotonNetwork.QuickResends = QuickResendAttempts;
        }
    }

    public void RestorePhotonConfig()
    {
        if(!_isSavedOriginalConfig)
        {
            return;
        }

        PhotonNetwork.photonMono.updateInterval = _originalUpdateInterval;
        PhotonNetwork.photonMono.updateIntervalOnSerialize = _originalUpdateIntervalOnSerialize;

        PhotonNetwork.MaxResendsBeforeDisconnect = _originalSentCountAllowance;
        PhotonNetwork.QuickResends = _originalQuickResendAttempts;

        PhotonNetwork.networkingPeer.MaximumTransferUnit = _originalMaximumTransferUnit;
    }

    public void RegisterOnGoingCommand()
    {
        _pendingOutgoingCommands = true;
    }

    public void SendOutgoingCommands()
    {
        //if (Enabled && _pendingOutgoingCommands)
        if (_pendingOutgoingCommands)
        {
            _pendingOutgoingCommands = false;
            PhotonNetwork.SendOutgoingCommands();
        }
    }
}

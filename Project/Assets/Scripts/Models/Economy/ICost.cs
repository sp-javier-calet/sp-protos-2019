
using SocialPoint.Base;
using System;

public interface ICost
{
    void Spend(Action<Error> finished);
}
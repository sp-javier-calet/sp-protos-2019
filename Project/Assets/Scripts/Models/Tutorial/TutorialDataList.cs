using System.Collections.Generic;
using SocialPoint.Tutorial;
using UnityEngine;

[CreateAssetMenu(fileName = "Tutorials", menuName = "Sparta/Tutorials")]
public class TutorialDataList : ScriptableObject
{
    public List<TutorialData> Tutorials;
}

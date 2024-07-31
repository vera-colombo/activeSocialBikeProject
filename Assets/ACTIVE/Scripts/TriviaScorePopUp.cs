using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A struct used to show new scope pop-ups.  Since the Score Value can sometimes be the same,
/// a NetworkBool is toggled to show that the score has been updated.
/// </summary>
public struct TriviaScorePopUp : INetworkStruct
{
    /// <summary>
    /// The score that will be displayed.  0 or less will show an "X"
    /// </summary>
    public int Score;

    /// <summary>
    /// The toggle value to force a change to this property, even if the score remains unchanged.
    /// </summary>
    public NetworkBool Toggle;
}

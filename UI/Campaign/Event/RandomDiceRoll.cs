using UnityEngine;

namespace TJ.Event
{
public class RandomDiceRoll : MonoBehaviour
{
    public int RollDice(System.Random seed)
    {
        int roll = seed.Next(1, 21);

        //50% chance to double the rolls less than 10 cause some of you mfers won't stop complaining about it being rigged. Well now it is I guess.
        if(roll <= 10 && seed.Next(0, 2) == 1) {
           roll *= 2;
        }

        return roll;
    }
}
}
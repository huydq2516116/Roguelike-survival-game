using SmallHedge.SoundManager;
using UnityEngine;

public class FoodObject : CellObject
{
    public int AmountGranted;
    public override void PlayerEntered()
    {
        Destroy(gameObject);

        //increase food
        GameManager.Instance.ChangeFood(AmountGranted);

        //Play Audio
        if (AmountGranted == 5)
        {
            SoundManager.PlaySound(SoundType.FRUIT, GameManager.Instance.audioSource);
        }
        if (AmountGranted == 15)
        {
            SoundManager.PlaySound(SoundType.SODA, GameManager.Instance.audioSource);
        }
    }
}

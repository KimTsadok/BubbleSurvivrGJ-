using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffsBehaviour : MonoBehaviour
{
    public BuffType buffType; // סוג ה-Buff שנבחר עבור ה-BuffPrefab

    // הפונקציה נקראת כאשר אובייקט אחר נכנס ל-collider של ה-BuffPrefab
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("OnTriggerEnter2D \n");

        // אם השחקן נכנס ל-Buff
        Debug.Log(collision.tag);
        if (collision.CompareTag("Player1"))
        {
            Debug.Log("CompareTag Player \n");
            // קרוא לפונקציה שתשפיע על ה-Buff של השחקן, תעביר את ה-BuffType כפרמטר
            BuffManagarBehaviour.Instance.ActivateBuff(buffType); // קריאה לפונקציה במנהל הבופים
            Destroy(gameObject); // אחרי שהשחקן נגע, ה-Buff ייהרס
        }
    }
}


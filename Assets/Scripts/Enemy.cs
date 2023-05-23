using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float Health { get; private set; } = 100.0f;

    public void ReduceHealth(float value)
    {
        Health -= value;
    }
}
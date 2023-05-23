using UnityEngine;

public class Actor : MonoBehaviour
{
    [Tooltip(
        "Represents the affiliation (or team) of the actor. Actors in the same affiliation are friendly to each other")]
    public int Affiliation;

    [Tooltip("Represents point where other actors will aim when they attack this actor")]
    public Transform AimPoint;

    private ActorsManager m_ActorsManager;

    // Start is called before the first frame update
    private void Start()
    {
        m_ActorsManager = FindObjectOfType<ActorsManager>();

        if (!m_ActorsManager.Actors.Contains(this)) m_ActorsManager.Actors.Add(this);
    }

    private void OnDestroy()
    {
        if (m_ActorsManager) m_ActorsManager.Actors.Remove(this);
    }
}
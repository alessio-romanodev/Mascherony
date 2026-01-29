using UnityEngine;
using System.Collections;

public class KillZone : MonoBehaviour
{
    public float fadeDuration = 0.2f;
    private CanvasGroup blackScreen;
    private bool isRespawning = false;

    private void Awake()
    {
        // trova automaticamente l'oggetto chiamato "Blackscreen"
        GameObject blackScreenObj = GameObject.Find("Blackscreen");
        if (blackScreenObj != null)
        {
            blackScreen = blackScreenObj.GetComponent<CanvasGroup>();

            if (blackScreen == null && blackScreenObj.transform.childCount > 0)
                blackScreen = blackScreenObj.transform.GetChild(0).GetComponent<CanvasGroup>();

            if (blackScreen == null)
                Debug.LogWarning("Blackscreen trovato ma non ha CanvasGroup!");
        }
        else
        {
            Debug.LogWarning("Oggetto Blackscreen non trovato in scena!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRespawning) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && player.lastCheckpoint != null)
        {
            StartCoroutine(RespawnPlayer(player));
        }
    }

    private IEnumerator RespawnPlayer(PlayerMovement player)
    {
        isRespawning = true;

        // Blocca input e movimento
        player.canMove = false;

        // Azzeriamo la velocity per evitare che il FixedUpdate sovrascriva la posizione
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = Vector3.zero;

        // Fade in nero
        if (blackScreen != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                blackScreen.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            blackScreen.alpha = 1f;
        }

        // Respawn player
        player.transform.position = player.lastCheckpoint.position;
        if (rb != null)
            rb.linearVelocity = Vector3.zero; // assicura che non si muova subito

        player.transform.position = player.lastCheckpoint.position;

        // Attendere un frame per stabilizzare la posizione
        yield return new WaitForSeconds(0.5f);
        // Fade out
        if (blackScreen != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                blackScreen.alpha = Mathf.Clamp01(1 - t / fadeDuration);
                yield return null;
            }
            blackScreen.alpha = 0f;
        }

        // Riabilita salto e buffer
        player.canMove = true;

        isRespawning = false;
    }
}

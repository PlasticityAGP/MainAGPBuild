using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class SCR_CharacterAudioTrigger : MonoBehaviour {
    private SCR_CharacterManager _characterManager;

    private void Start() {
        var mgr = GetComponentInParent<SCR_CharacterManager>();
        if (mgr == null) {
            Debug.LogWarning("CharacterAudioTrigger could not find CharacterManager in its parent.");
            return;
        }
        _characterManager = mgr;
    }

    public void AudioTrigger(string name) {
        if (_characterManager) {
            _characterManager.HandleAudioEvent(name);
        }
    }
}
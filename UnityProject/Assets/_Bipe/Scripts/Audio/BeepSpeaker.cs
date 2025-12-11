using System.Collections.Generic;
using UnityEngine;

namespace Bipe
{
    // PC speaker: builds a square-wave clip from a pattern like "L S S"
    public class BeepSpeaker : MonoBehaviour
    {
        const int Rate = 44100;
        const float Freq = 880f, Volume = 0.12f, Short = 0.14f, Long = 0.55f, Gap = 0.22f;

        AudioSource source;

        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }

        public void Play(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return;
            var data = new List<float>();
            int period = Mathf.RoundToInt(Rate / Freq);
            foreach (var token in pattern.Split(' '))
            {
                int tone = Mathf.RoundToInt(Rate * (token == "L" ? Long : Short));
                for (int i = 0; i < tone; i++) data.Add(i % period < period / 2 ? Volume : -Volume);
                int gap = Mathf.RoundToInt(Rate * Gap);
                for (int i = 0; i < gap; i++) data.Add(0f);
            }
            var clip = AudioClip.Create("bipe", data.Count, 1, Rate, false);
            clip.SetData(data.ToArray(), 0);
            source.PlayOneShot(clip);
        }
    }
}

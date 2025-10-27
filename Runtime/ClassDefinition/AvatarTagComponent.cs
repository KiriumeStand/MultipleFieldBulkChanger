using System;
using UnityEngine;
using VRC.SDKBase;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    public abstract class AvatarTagComponent : MonoBehaviour, IEditorOnly
    {
        void Start()
        {
            // ここは消さない
            // 消すとインスペクターでenabledのチェックボックスが消える}
        }
    }
}
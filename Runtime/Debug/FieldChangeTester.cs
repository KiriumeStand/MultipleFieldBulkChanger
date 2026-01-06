using System;
using System.Collections.Generic;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    [Serializable]
    public class FieldChangeTester : MonoBehaviour
    {
        void Start()
        {
            // ここは消さない
            // 消すとインスペクターでenabledのチェックボックスが消える
        }

        public List<UnityEngine.Object> TargetObjects = new();

        public bool _TestBool1 = false;
        public bool _TestBool2 = true;

        public double _TestDouble1 = 0.123;
        public double _TestDouble2 = 0.987;

        public byte _TestByte1 = 2;
        public byte _TestByte2 = 3;

        public sbyte _TestSByte1 = 2;
        public sbyte _TestSByte2 = 3;

        public ushort _TestUShort1 = 2;
        public ushort _TestUShort2 = 3;

        public short _TestShort1 = 2;
        public short _TestShort2 = 3;

        public uint _TestUInt1 = 2;
        public uint _TestUInt2 = 3;

        public int _TestInt1 = 123;
        public int _TestInt2 = 987;

        public ulong _TestULong1 = 1234UL;
        public ulong _TestULong2 = 9876UL;

        public long _TestLong1 = 1234L;
        public long _TestLong2 = 9876L;

        public int _TestEnum1 = 2;
        public int _TestEnum2 = 3;

        public string _TestString1 = "TestString1";
        public string _TestString2 = "StringTest2";

        public char _TestChar1 = 'K';
        public char _TestChar2 = 'S';

        public Vector2 _TestVector21 = new(0.11F, 0.22F);
        public Vector2 _TestVector22 = new(0.44F, 0.33F);

        public Vector3 _TestVector31 = new(0.11F, 0.22F, 0.33F);
        public Vector3 _TestVector32 = new(0.44F, 0.33F, 0.22F);

        public Vector4 _TestVector41 = new(0.11F, 0.22F, 0.33F, 0.44F);
        public Vector4 _TestVector42 = new(0.44F, 0.33F, 0.22F, 0.11F);

        public Bounds _TestBounds1 = new(new(1.2F, 2.3F, 3.4F), new(4.5F, 5.6F, 6.7F));
        public Bounds _TestBounds2 = new(new(9.8F, 7.6F, 5.4F), new(1.2F, 3.4F, 5.6F));

        public Color _TestColor1 = new(0.31f, 0.31f, 0.31f, 0.31f);
        public Color _TestColor2 = new(0.69f, 0.69f, 0.69f, 0.69f);

        public AnimationCurve _TestCurve1 = new(new Keyframe[3] { new(0.0F, 0.0F), new(0.5F, 1.0F), new(1.0F, 0.5F) });
        public AnimationCurve _TestCurve2 = new(new Keyframe[3] { new(0.0F, 0.5F), new(0.5F, 0.0F), new(1.0F, 1.0F) });

        public Gradient _TestGradient1 = new();
        public Gradient _TestGradient2 = new();

        public List<UnityEngine.Object> _TestObjects = new();

        public List<(object, object)> TestValueList
        {
            get
            {
                List<(object, object)> lists = new()
                {
                    ( _TestBool1, _TestBool2 ),
                    ( _TestDouble1, _TestDouble2 ),
                    ( _TestByte1, _TestByte2 ),
                    ( _TestSByte1, _TestSByte2 ),
                    ( _TestUShort1, _TestUShort2 ),
                    ( _TestShort1, _TestShort2 ),
                    ( _TestUInt1, _TestUInt2 ),
                    ( _TestInt1 , _TestInt2 ),
                    ( _TestULong1 , _TestULong2 ),
                    ( _TestLong1 , _TestLong2 ),
                    ( _TestEnum1, _TestEnum2 ),
                    ( _TestString1, _TestString2 ),
                    ( _TestChar1, _TestChar2 ),
                    ( _TestVector21, _TestVector22 ),
                    ( _TestVector31, _TestVector32 ),
                    ( _TestVector41, _TestVector42 ),
                    ( _TestBounds1, _TestBounds2 ),
                    ( _TestColor1, _TestColor2 ),
                    ( _TestCurve1, _TestCurve2 ),
                    ( _TestGradient1, _TestGradient2 ),
                };
                return lists;
            }
        }
    }
}
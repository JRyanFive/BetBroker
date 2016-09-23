using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BcWin.Server
{
    public class Base64Encoder
    {

        //public static const long MAX_BUFFER_SIZE = 32767;
        //private static const Array ALPHABET_CHAR_CODES = new Array[65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 43, 47];
        //public static const string CHARSET_UTF_8 = "UTF-8";
        //private static const int ESCAPE_CHAR_CODE = 61;

        //public static int newLine = 10;

        //private long _line;
        //private long _count;
        //private List<int> _buffers;
        //public bool insertNewLines = true;
        //private int[] _work;

        public Base64Encoder(){
            //_work = new int[]{};
            //super();
            //reset();
        }
        //public string flush(){
        //    if (_count > 0){
        //        encodeBlock();
        //    };
        //    var _local1 = drain();
        //    reset();
        //    return (_local1);
        //}
        //public string toString(){
        //    return (flush());
        //}
        //public void reset(){           
        //    _buffers = new List<int>();
        //    _count = 0;
        //    _line = 0;
        //    _work[0] = 0;
        //    _work[1] = 0;
        //    _work[2] = 0;
            
        //}
        //public void encodeBytes(_arg1:ByteArray, _arg2:uint=0, _arg3:uint=0):void{
        //    if (_arg3 == 0){
        //        _arg3 = _arg1.length;
        //    };
        //    var _local4:uint = _arg1.position;
        //    _arg1.position = _arg2;
        //    var _local5:uint = _arg2;
        //    var _local6:uint = (_arg2 + _arg3);
        //    if (_local6 > _arg1.length){
        //        _local6 = _arg1.length;
        //    };
        //    while (_local5 < _local6) {
        //        _work[_count] = _arg1[_local5];
        //        _count++;
        //        if ((((_count == _work.length)) || (((_local6 - _local5) == 1)))){
        //            encodeBlock();
        //            _count = 0;
        //            _work[0] = 0;
        //            _work[1] = 0;
        //            _work[2] = 0;
        //        };
        //        _local5++;
        //    };
        //    _arg1.position = _local4;
        //}
        public void encode(string _arg1, long _arg2 = 0, long _arg3 = 0){
            //if (_arg3 == 0){
            //    _arg3 = _arg1.Length;
            //};
            //var _local4 = _arg2;
            //var _local5 = (_arg2 + _arg3);
            //if (_local5 > _arg1.Length){
            //    _local5 = _arg1.Length;
            //};
            //while (_local4 < _local5) {
              
            //    _work[_count] = _arg1.charCodeAt(_local4);
            //    _count++;
            //    if ((((_count == _work.length)) || (((_local5 - _local4) == 1)))){
            //        encodeBlock();
            //        _count = 0;
            //        _work[0] = 0;
            //        _work[1] = 0;
            //        _work[2] = 0;
            //    };
            //    _local4++;
            //};
        }
        //private function encodeBlock():void{
        //    var _local1:Array = (_buffers[(_buffers.length - 1)] as Array);
        //    if (_local1.length >= MAX_BUFFER_SIZE){
        //        _local1 = [];
        //        _buffers.push(_local1);
        //    };
        //    _local1.push(ALPHABET_CHAR_CODES[((_work[0] & 0xFF) >> 2)]);
        //    _local1.push(ALPHABET_CHAR_CODES[(((_work[0] & 3) << 4) | ((_work[1] & 240) >> 4))]);
        //    if (_count > 1){
        //        _local1.push(ALPHABET_CHAR_CODES[(((_work[1] & 15) << 2) | ((_work[2] & 192) >> 6))]);
        //    } else {
        //        _local1.push(ESCAPE_CHAR_CODE);
        //    };
        //    if (_count > 2){
        //        _local1.push(ALPHABET_CHAR_CODES[(_work[2] & 63)]);
        //    } else {
        //        _local1.push(ESCAPE_CHAR_CODE);
        //    };
        //    if (insertNewLines){
        //        if ((_line = (_line + 4)) == 76){
        //            _local1.push(newLine);
        //            _line = 0;
        //        };
        //    };
        //}
        //public function encodeUTFBytes(_arg1:String):void{
        //    var _local2:ByteArray = new ByteArray();
        //    _local2.writeUTFBytes(_arg1);
        //    _local2.position = 0;
        //    encodeBytes(_local2);
        //}
        //public function drain():String{
        //    var _local3:Array;
        //    var _local1 = "";
        //    var _local2:uint;
        //    while (_local2 < _buffers.length) {
        //        _local3 = (_buffers[_local2] as Array);
        //        _local1 = (_local1 + String.fromCharCode.apply(null, _local3));
        //        _local2++;
        //    };
        //    _buffers = [];
        //    _buffers.push([]);
        //    return (_local1);
        //}

    }
}

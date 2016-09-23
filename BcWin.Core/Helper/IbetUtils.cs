using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint;

namespace BcWin.Core.Helper
{
    public class IbetUtils
    {
        public string hashPass(string username, string password, string code)
        {
            //var engine = new Jurassic.ScriptEngine();
            //   engine.ExecuteFile("Common.js");

            var h = username;
            var i = password;
            var c = code;


            // var b = engine.CallGlobalFunction<string>("CFS", i);

            var b = CFS(i);
            var d = b + c;

            //var ccc = engine.CallGlobalFunction<object>("MD5", d);
            return MD5(d);

            // return null;
        }

        //public static object CFS(string k)
        //{
        //    try
        //    {

        //        return null;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}

        //public static object MD5(string W)
        //{
        //    return null;
        //}

        public string CFS(string k)
        {

            try
            {
                var engine = new Engine().Execute("function CFS (k) { function e(q) { var r = ''; for (var o = 1; o <= q.length; o++) { r += q.charAt(o - 1).charCodeAt(0) } var p = new Number(r); r = p.toString(16); return r } var g = 30, h, a; h = g - k.length; if (h > 1) { for (var d = 1; d <= h; d++) { k = k + String.fromCharCode(21) } } var l = new Number(1); for (var c = 1; c <= g; c++) { a = g + k.charAt(c - 1).charCodeAt(0) * c; l = l * a } var n = new Number(l.toPrecision(15)); k = n.toString().toUpperCase(); var m = ''; for (var b = 1; b <= k.length; b++) { m = m + e(k.substring(b - 1, b + 2)) } var f = ''; for (var b = 20; b <= m.length - 18; b += 2) { f = f + m.charAt(b - 1) } return f.toUpperCase() };");
                var aa = engine.Invoke("CFS", k);
                return aa.ToString();

                //return bbb;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public string MD5(string W)
        {

            try
            {
                var engine = new Engine()
              .Execute(@"function MD5(W) { function WordToHex(c) { var d = '', e = '', a, b; for (b = 0; b <= 3; b++) { a = (c >>> (b * 8)) & 255; e = '0' + a.toString(16); d = d + e.substr(e.length - 2, 2) } return d } function B(b, a) { return (b << a) | (b >>> (32 - a)) } function g(b, k) { var c, x, d, F, a; d = (b & 2147483648); F = (k & 2147483648); c = (b & 1073741824); x = (k & 1073741824); a = (b & 1073741823) + (k & 1073741823); if (c & x) { return (a ^ 2147483648 ^ d ^ F) } if (c | x) { if (a & 1073741824) { return (a ^ 3221225472 ^ d ^ F) } else { return (a ^ 1073741824 ^ d ^ F) } } else { return (a ^ d ^ F) } } function r(a, b, c) { return (a & b) | ((~a) & c) } function t(a, b, c) { return (a & c) | (b & (~c)) } function v(a, b, c) { return (a ^ b ^ c) } function y(a, b, c) { return (b ^ (a | (~c))) } function s(k, G, H, I, ab, aa, F) { k = g(k, g(g(r(G, H, I), ab), F)); return g(B(k, aa), G) } function u(k, G, H, I, ab, aa, F) { k = g(k, g(g(t(G, H, I), ab), F)); return g(B(k, aa), G) } function w(k, G, H, I, ab, aa, F) { k = g(k, g(g(v(G, H, I), ab), F)); return g(B(k, aa), G) } function z(k, G, H, I, ab, aa, F) { k = g(k, g(g(y(G, H, I), ab), F)); return g(B(k, aa), G) } function o(H) { var G; var c = H.length; var k = c + 8; var x = (k - (k % 64)) / 64; var d = (x + 1) * 16; var F = Array(d - 1); var b = 0; var a = 0; while (a < c) { G = (a - (a % 4)) / 4; b = (a % 4) * 8; F[G] = (F[G] | (H.charCodeAt(a) << b)); a++ } G = (a - (a % 4)) / 4; b = (a % 4) * 8; F[G] = F[G] | (128 << b); F[d - 2] = c << 3; F[d - 1] = c >>> 29; return F } function Y(d) { d = d.replace(/\r\n/g, '\n'); var k = ''; for (var b = 0; b < d.length; b++) { var a = d.charCodeAt(b); if (a < 128) { k += String.fromCharCode(a) } else { if ((a > 127) && (a < 2048)) { k += String.fromCharCode((a >> 6) | 192); k += String.fromCharCode((a & 63) | 128) } else { k += String.fromCharCode((a >> 12) | 224); k += String.fromCharCode(((a >> 6) & 63) | 128); k += String.fromCharCode((a & 63) | 128) } } } return k } var Z = Array(); var A, f, l, n, q, e, h, m, p; var C = 7, D = 12, E = 17, J = 22; var K = 5, L = 9, M = 14, N = 20; var O = 4, P = 11, Q = 16, R = 23; var S = 6, T = 10, U = 15, V = 21; W = Y(W); Z = o(W); e = 1732584193; h = 4023233417; m = 2562383102; p = 271733878; for (A = 0; A < Z.length; A += 16) { f = e; l = h; n = m; q = p; e = s(e, h, m, p, Z[A + 0], C, 3614090360); p = s(p, e, h, m, Z[A + 1], D, 3905402710); m = s(m, p, e, h, Z[A + 2], E, 606105819); h = s(h, m, p, e, Z[A + 3], J, 3250441966); e = s(e, h, m, p, Z[A + 4], C, 4118548399); p = s(p, e, h, m, Z[A + 5], D, 1200080426); m = s(m, p, e, h, Z[A + 6], E, 2821735955); h = s(h, m, p, e, Z[A + 7], J, 4249261313); e = s(e, h, m, p, Z[A + 8], C, 1770035416); p = s(p, e, h, m, Z[A + 9], D, 2336552879); m = s(m, p, e, h, Z[A + 10], E, 4294925233); h = s(h, m, p, e, Z[A + 11], J, 2304563134); e = s(e, h, m, p, Z[A + 12], C, 1804603682); p = s(p, e, h, m, Z[A + 13], D, 4254626195); m = s(m, p, e, h, Z[A + 14], E, 2792965006); h = s(h, m, p, e, Z[A + 15], J, 1236535329); e = u(e, h, m, p, Z[A + 1], K, 4129170786); p = u(p, e, h, m, Z[A + 6], L, 3225465664); m = u(m, p, e, h, Z[A + 11], M, 643717713); h = u(h, m, p, e, Z[A + 0], N, 3921069994); e = u(e, h, m, p, Z[A + 5], K, 3593408605); p = u(p, e, h, m, Z[A + 10], L, 38016083); m = u(m, p, e, h, Z[A + 15], M, 3634488961); h = u(h, m, p, e, Z[A + 4], N, 3889429448); e = u(e, h, m, p, Z[A + 9], K, 568446438); p = u(p, e, h, m, Z[A + 14], L, 3275163606); m = u(m, p, e, h, Z[A + 3], M, 4107603335); h = u(h, m, p, e, Z[A + 8], N, 1163531501); e = u(e, h, m, p, Z[A + 13], K, 2850285829); p = u(p, e, h, m, Z[A + 2], L, 4243563512); m = u(m, p, e, h, Z[A + 7], M, 1735328473); h = u(h, m, p, e, Z[A + 12], N, 2368359562); e = w(e, h, m, p, Z[A + 5], O, 4294588738); p = w(p, e, h, m, Z[A + 8], P, 2272392833); m = w(m, p, e, h, Z[A + 11], Q, 1839030562); h = w(h, m, p, e, Z[A + 14], R, 4259657740); e = w(e, h, m, p, Z[A + 1], O, 2763975236); p = w(p, e, h, m, Z[A + 4], P, 1272893353); m = w(m, p, e, h, Z[A + 7], Q, 4139469664); h = w(h, m, p, e, Z[A + 10], R, 3200236656); e = w(e, h, m, p, Z[A + 13], O, 681279174); p = w(p, e, h, m, Z[A + 0], P, 3936430074); m = w(m, p, e, h, Z[A + 3], Q, 3572445317); h = w(h, m, p, e, Z[A + 6], R, 76029189); e = w(e, h, m, p, Z[A + 9], O, 3654602809); p = w(p, e, h, m, Z[A + 12], P, 3873151461); m = w(m, p, e, h, Z[A + 15], Q, 530742520); h = w(h, m, p, e, Z[A + 2], R, 3299628645); e = z(e, h, m, p, Z[A + 0], S, 4096336452); p = z(p, e, h, m, Z[A + 7], T, 1126891415); m = z(m, p, e, h, Z[A + 14], U, 2878612391); h = z(h, m, p, e, Z[A + 5], V, 4237533241); e = z(e, h, m, p, Z[A + 12], S, 1700485571); p = z(p, e, h, m, Z[A + 3], T, 2399980690); m = z(m, p, e, h, Z[A + 10], U, 4293915773); h = z(h, m, p, e, Z[A + 1], V, 2240044497); e = z(e, h, m, p, Z[A + 8], S, 1873313359); p = z(p, e, h, m, Z[A + 15], T, 4264355552); m = z(m, p, e, h, Z[A + 6], U, 2734768916); h = z(h, m, p, e, Z[A + 13], V, 1309151649); e = z(e, h, m, p, Z[A + 4], S, 4149444226); p = z(p, e, h, m, Z[A + 11], T, 3174756917); m = z(m, p, e, h, Z[A + 2], U, 718787259); h = z(h, m, p, e, Z[A + 9], V, 3951481745); e = g(e, f); h = g(h, l); m = g(m, n); p = g(p, q) } var X = WordToHex(e) + WordToHex(h) + WordToHex(m) + WordToHex(p); return X.toLowerCase() }; ");

                var bbb = engine.Invoke("MD5", W);
                return bbb.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

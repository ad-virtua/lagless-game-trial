using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Generic
{
    /// <summary>
    /// ダメージを受けた際の点滅処理
    /// </summary>
    /// <returns>0.1秒ごとに表示非表示を切り替える</returns>
    public static IEnumerator DamageFlash(SpriteRenderer sr, float waitTime, int flashing = 4)
    {
        for (int i = 0; i < flashing; i++)
        {
            yield return new WaitForSeconds(waitTime);

            sr.enabled = !sr.enabled;
        }
    }



    /// <summary>
    /// 現在時刻と保存している時刻を比較して指定時間経過したかを確認
    /// </summary>
    /// <param name="saveTime">比較する時間</param>
    /// <param name="targetTime">数えたい時間</param>
    /// <returns></returns>
    public static bool IsDayTimeCount(int[] saveTime, TimeSpan targetTime)
    {
        TimeSpan distance
            = new TimeSpan(
                DateTime.Now.Day - saveTime[0],
                DateTime.Now.Hour - saveTime[1],
                DateTime.Now.Minute - saveTime[2],
                DateTime.Now.Second - saveTime[3]);

        if (distance > targetTime) return true;
        else return false;
    }



    /// <summary>
    /// 現在時刻と保存している時刻を比較した結果を返す
    /// </summary>
    /// <param name="saveTime">比較する時間</param>
    /// <returns></returns>
    public static TimeSpan DayTimeSpan(int[] saveTime)
    {
        TimeSpan distance
            = new TimeSpan(
                DateTime.Now.Day - saveTime[0],
                DateTime.Now.Hour - saveTime[1],
                DateTime.Now.Minute - saveTime[2],
                DateTime.Now.Second - saveTime[3]);
        return distance;
    }



    /// <summary>
    /// 不要なオブジェクトをタグで削除
    /// </summary>
    /// <param name="tagName">タグの名前</param>
    public static void DestroyTag(string tagName)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag(tagName)) { GameObject.Destroy(obj); }
    }



    private static bool isShake = false;
    /// <summary>
    /// オブジェクトを揺らす
    /// </summary>
    /// <param name="duration">  持続時間                  </param>
    /// <param name="magnitude"> 揺れの大きさ              </param>
    /// <param name="obj">       揺らす対象のオブジェクト  </param>
    /// <param name="isUI">      UIなのかどうか            </param>
    /// <returns>                0.1秒ごとに処理を繰り返す </returns>
    public static IEnumerator Shake(float duration, float magnitude, GameObject obj, bool isUI = false)
    {
        while(isShake) yield return null;
        isShake = true;

        Vector3 pos;

        if (!isUI) { pos = obj.transform.localPosition; }
        else { pos = obj.GetComponent<RectTransform>().position; }

        var elapsed = 0f;

        while (elapsed < duration)
        {
            var x = pos.x + UnityEngine.Random.Range(-1f, 1f) * magnitude;
            var y = pos.y + UnityEngine.Random.Range(-1f, 1f) * magnitude;

            if (!isUI) { obj.transform.localPosition = new Vector3(x, y, pos.z); }
            else { obj.GetComponent<RectTransform>().position = new Vector3(x, y, pos.z); }

            elapsed += Time.deltaTime;

            yield return null;
        }

        if (!isUI) { obj.transform.localPosition = pos; }
        else { obj.GetComponent<RectTransform>().position = pos; }

        isShake = false;
    }


    /// <summary>
    /// 2次ベジェ曲線
    /// </summary>
    /// <param name="p0">開始点</param>
    /// <param name="p1">中間点</param>
    /// <param name="p2">終着点</param>
    /// <param name="t">Lerp(a,b)どっちによっているかの比率(a = 0,b = 1)</param>
    /// <returns>実際に移動する座標</returns>
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        var a = Vector3.Lerp(p0, p1, t); // p0からp1の動き
        var b = Vector3.Lerp(p1, p2, t); // p1からp2の動き

        return Vector3.Lerp(a, b, t); // aからbの動き
    }

    /// <summary>
    /// 目的の値に最も近い値を返します
    /// </summary>
    public static float Nearest(
        this IEnumerable<float> self,
        float target
    )
    {
        var min = self.Min(c => Math.Abs(c - target));
        return self.First(c => Math.Abs(c - target) == min);
    }

    /// <summary>
    /// floatをstringに変換する
    /// </summary>
    /// <param name="values">値の配列</param>
    /// <returns>string変換した値</returns>
    public static string FloatsChangeString(float[] values)
    {
        if (values.Length == 0) return "";

        string data = "";

        foreach (float value in values)
        {
            data += value.ToString() + "@";
        }
        data = data.Remove(data.Length - 1); //一番後ろの@は削除

        return data;
    }

    private const string PASSWORD_CHARS =
    "0123456789abcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// 指定文字数のランダムな文字列を返す(ランダムパスワード)
    /// </summary>
    public static string GeneratePassword(int length)
    {
        var sb = new System.Text.StringBuilder(length);
        var r = new System.Random();

        for (int i = 0; i < length; i++)
        {
            int pos = r.Next(PASSWORD_CHARS.Length);
            char c = PASSWORD_CHARS[pos];
            sb.Append(c);
        }

        return sb.ToString();
    }

    public static IEnumerator BigupObj(GameObject bigObj, float addScale, float time)
    {
        Vector3 bigScale = new Vector3(
            bigObj.transform.localScale.x + addScale,
            bigObj.transform.localScale.y + addScale,
            bigObj.transform.localScale.z + addScale);

        while (bigObj.transform.localScale.x < bigScale.x)
        {
            bigObj.transform.localScale +=
                new Vector3(addScale / (time * 60.0f),
                            addScale / (time * 60.0f),
                            addScale / (time * 60.0f));

            yield return null;
        }
    }

    public static IEnumerator SmallupObj(GameObject smallObj, float minusScale, float time)
    {
        Vector3 smallScale = new Vector3(
            smallObj.transform.localScale.x - minusScale,
            smallObj.transform.localScale.y - minusScale,
            smallObj.transform.localScale.z - minusScale);

        while (smallObj.transform.localScale.x > smallScale.x)
        {
            smallObj.transform.localScale -=
                new Vector3(minusScale / (time * 60.0f),
                            minusScale / (time * 60.0f),
                            minusScale / (time * 60.0f));

            yield return null;
        }
    }
}
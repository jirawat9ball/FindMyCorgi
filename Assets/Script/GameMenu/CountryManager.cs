using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class CountryManager : MonoBehaviour

{

    [Header("UI Managers References")]

    [Tooltip("ʤԻѺ͹ѹ͹Ҩ")]

    public SlideInObject slideInObject;



    [Tooltip("ʤԻѺҴҹ")]

    public UILineRenderer uILineRenderer;



    [Tooltip("ʤԻѺѴԴ/ԴǴҹ")]

    public UIMapScene uIMapScene;



    [Header("Game Data")]

    [Tooltip("ӹǹҹ蹻Ŵͤǵ͹ ( 3 ͻŴͤҹ 0, 1, 2)")]

    public int currentUnlockedCount = 3;



    /// <summary>

    /// ѧѹèѴἹ (ö¡ӨҡʤԻͼҹҹ)

    /// </summary>

    public void SetupMapSequence(int unlockedCount)

    {

        currentUnlockedCount = unlockedCount;

        StartCoroutine(MapRoutine(currentUnlockedCount));

    }



    // Coroutine ѺѧѺӧҹӴѺ (Step-by-step)

    private IEnumerator MapRoutine(int unlockedCount)

    {

        // ==========================================

        // 绷 1: ԴҹŴͤ

        // ==========================================

        uIMapScene.SetAllDisable(); // Դءҹ͹

        uIMapScene.gameObject.SetActive(false); // ԴἹ繡͹ (ҹѧԴ)

        if (uILineRenderer != null)

        {

            uILineRenderer.drawProgress = 0f;

            uILineRenderer.SetAllDirty();

        }



        List<RectTransform> activePoints = new List<RectTransform>();

        // ==========================================

        // 绷 3: ͹ѹ͹ἹҨ

        // ==========================================

        if (slideInObject != null)

        {

            // ¡ԡѹѵѵԵ͹ ( 1 Ѻ 2 稡͹)

            slideInObject.moveDuration = 0.25f;

            slideInObject.MoveIn();

        }

        yield return new WaitForSeconds(0.25f);

        // ==========================================

        // 绷 2: ѻവҴҹ

        // ==========================================

        // ӨشԴ ¹ʤԻҴ

        if (uILineRenderer != null)

        {

            StartCoroutine(uILineRenderer.AnimateLine(0.25f)); // ͹ѹҴ ( 1 Թҷ)

        }



        //  Unity ѻവ˹ҵ UI Ҵ 1 

        yield return new WaitForSeconds(0.15f);

        uIMapScene.gameObject.SetActive(true); // ԴἹ繡͹ (ҹѧԴ)

        yield return new WaitForSeconds(0.25f);



        // ǹٻԴҹӹǹŴͤ

        for (int i = 0; i < unlockedCount; i++)

        {

            // 礡ѹ˹ ҹŴͤԹӹǹըԧ

            if (i < uIMapScene.parrent.Length && uIMapScene.parrent[i] != null)

            {

                uIMapScene.SetActive(i); // Դҹ



                // ֧ RectTransform ͧҹҴ

                RectTransform rect = uIMapScene.parrent[i].GetComponent<RectTransform>();

                if (rect != null)

                {

                    activePoints.Add(rect);

                }

                yield return new WaitForSeconds(0.15f);



            }

        }



       



        

    }

}
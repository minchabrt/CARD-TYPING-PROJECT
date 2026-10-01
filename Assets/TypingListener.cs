using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TypingListener : MonoBehaviour
{
    public TextMeshProUGUI wordText;
    public string targetWord = "fireball";

    public Volume globalVolume;
    private Vignette vignette;

    [Header("Vignette")]
    public float baseVignetteIntensity = 0.427f; // default vrednost (van kucanja)
    public float maxVignetteIntensity = 0.5f;    // vrednost na pun progres kucanja

    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public Color defaultColor = Color.black;

    [Header("Tajmeri")]
    public float errorResetDelay = 1f;    // koliko dugo ostaje crveno pre reseta
    public float completeResetDelay = 3f; // koliko dugo čeka posle uspešnog završetka pre reseta

    [Header("Aktivacija")]
    public CardSystem cameraScript;
    public int myCardIndex = 0;        // 0 = card1, 1 = card2, 2 = card3, 3 = card4

    private int typedIndex = 0;  // koliko slova je igrač do sad tačno otkucao
    private int errorIndex = -1; // pozicija slova koje je trenutno crveno (-1 = nema greške)
    private bool isLocked = false; // dok je true, ignoriše se dalji unos (u toku je reset-čekanje)

    public float TypingProgress { get; private set; } = 0f; // 0 = ništa otkucano, 1 = cela reč tačna

    void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet<Vignette>(out vignette))
        {
            vignette.intensity.overrideState = true;
            vignette.intensity.value = baseVignetteIntensity;
        }

        UpdateWordDisplay();
    }

    void Update()
    {
        // radi samo ako igrač gleda dole I ako je BAŠ ova kartica selektovana
        bool isActive = cameraScript.isLookingDown
             && cameraScript.selectedCardIndex == myCardIndex
             && myCardIndex < cameraScript.cardCount;
        if (!isActive) return;

        // dok je zaključano (crveno posle greške, ili čekanje posle uspeha), ignoriši unos
        if (isLocked) return;

        foreach (char c in Input.inputString)
        {
            HandleTypedChar(c);
        }
    }

    void HandleTypedChar(char typedChar)
    {
        if (typedChar == '\b') return;
        if (typedIndex >= targetWord.Length) return;

        char expectedChar = targetWord[typedIndex];

        if (char.ToLower(typedChar) == char.ToLower(expectedChar))
        {
            // Tačno slovo - pomeri se dalje
            typedIndex++;
            TypingProgress = (float)typedIndex / targetWord.Length;

            if (typedIndex >= targetWord.Length)
            {
                UpdateWordDisplay();
                OnWordCompleted();
                return;
            }
        }
        else
        {
            // Pogrešno slovo - obeleži ga crveno, ODMAH resetuj scale progress, i zaključaj unos dok se ne resetuje
            errorIndex = typedIndex;
            isLocked = true;
            TypingProgress = 0f;
            UpdateWordDisplay();
            StartCoroutine(ResetAfterError());
            return;
        }

        UpdateWordDisplay();
    }

    IEnumerator ResetAfterError()
    {
        yield return new WaitForSeconds(errorResetDelay);

        typedIndex = 0;
        errorIndex = -1;
        isLocked = false;

        if (vignette != null)
        {
            vignette.intensity.value = baseVignetteIntensity;
        }

        UpdateWordDisplay();
    }

    void UpdateWordDisplay()
    {
        string result = "";

        for (int i = 0; i < targetWord.Length; i++)
        {
            char letter = targetWord[i];
            Color color;

            if (i == errorIndex)
            {
                color = wrongColor;
            }
            else if (i < typedIndex)
            {
                color = correctColor;
            }
            else
            {
                color = defaultColor;
            }

            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            result += $"<color=#{hexColor}>{letter}</color>";
        }

        if (wordText != null)
{
    wordText.text = result;
}

        // Vinjeta ide od baseVignetteIntensity (progres 0) do maxVignetteIntensity (progres 1) - POSTAVLJA se, ne akumulira
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(baseVignetteIntensity, maxVignetteIntensity, TypingProgress);
        }
    }

public void SetWord(string newWord)
{
    targetWord = newWord;
    typedIndex = 0;
    errorIndex = -1;
    TypingProgress = 0f;
    UpdateWordDisplay();
}


    void OnWordCompleted()
    {
        Debug.Log(targetWord + " otkucano! Ovde pozivaš fireball efekat.");
        // npr: cameraScript.SpawnCard1Effect(); ili šta god okidaš kad se reč tačno otkuca

        TypingProgress = 0f; // ODMAH resetuj scale, boje slova ostaju zelene do isteka completeResetDelay
        isLocked = true;
        StartCoroutine(ResetAfterComplete());

        // Vinjeta se odmah vraća na default (nezavisno od toga što slova ostaju zelena)
        if (vignette != null)
        {
            vignette.intensity.value = baseVignetteIntensity;
        }
    }

    IEnumerator ResetAfterComplete()
    {
        yield return new WaitForSeconds(completeResetDelay);

        typedIndex = 0;
        errorIndex = -1;
        isLocked = false;

        UpdateWordDisplay();
    }
}
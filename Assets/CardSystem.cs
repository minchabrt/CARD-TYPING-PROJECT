using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class CardSystem : MonoBehaviour
        {
        


[Header("Card Scaling")]

public Transform card1;
public Transform card2;
public Transform card3;
public Transform card4;

public float lookDownThreshold;
public float normalCardScale = 2f;      // default veličina kartice (kao u Inspectoru)
public float enlargedCardScale = 2.3f;  // veličina SVIH kartica kad se podignu (stari sistem, netaknut)
public float selectedExtraScale = 0.3f; // DODATNO uvećanje za selektovanu karticu, preko enlargedCardScale
public float cardScaleSpeed = 8f;

[Header("Typing Progress Scale")]
public TypingListener card1Typing;
public TypingListener card2Typing;
public TypingListener card3Typing;
public TypingListener card4Typing;
public float typingBoostAmount = 0.15f;  // koliko dodatno naraste selektovana kartica dok kucaš (na max progresu)
public float typingShrinkAmount = 0.1f;  // koliko se smanje ostale kartice dok kucaš (na max progresu)

[Header("Card Position")]
public float liftSpeed = 8f;
public float liftOffsetY = 0f; // za koliko se svaka kartica pomeri gore (dodaje se na njenu startnu Y poziciju)

[Header("Card Selector")]
public Image card1Selector;
public Image card2Selector;
public Image card3Selector;
public Image card4Selector;
public float selectorFadeSpeed = 8f;

[Header("Card Scroll SFX")]
public AudioSource sfxSource; // prevuci AudioSource komponentu (npr. sa Player objekta)
public AudioClip[] scrollSounds; // ubaci sva 4 zvuka ovde u Inspector-u

[Header("Camera Shake")]
public Transform cameraTransform; // prevuci istu kameru koju koristi FPMovement (playerCamera)
public float maxShakeAmount = 0.05f; // koliko jako trese na max progresu (svetske jedinice)
public float shakeFrequency = 25f; // koliko brzo se menja Perlin noise (veće = brže treskanje)
private Vector3 cameraBaseLocalPos;

private Vector3 card1StartPos, card2StartPos, card3StartPos, card4StartPos;

public int selectedCardIndex = 0; // koja je kartica trenutno "uvećana" (0 = card1, 1 = card2, 2 = card3, 3 = card4)
public bool isLookingDown = false; // da li igrač trenutno gleda dole (ažurira se svaki frame u Update())

[Header("Card Count")]
public int cardCount = 2;          // počinješ sa 2 kartice
public int maxCards = 4;
public Image[] cardImages = new Image[4]; // Image komponente kartica 1-4 (one koje prikazuju sprite)


        void Start(){

            card1StartPos = card1.localPosition;
            card2StartPos = card2.localPosition;
            card3StartPos = card3.localPosition;
            card4StartPos = card4.localPosition;

            if (cameraTransform != null)
            {
                cameraBaseLocalPos = cameraTransform.localPosition;
            }

        }

        void ApplyCameraShake(float progress)
        {
            if (cameraTransform == null) return;

            if (progress > 0f)
            {
                float shakeAmount = progress * maxShakeAmount;

                float noiseX = Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) * 2f - 1f;
                float noiseY = Mathf.PerlinNoise(0f, Time.time * shakeFrequency) * 2f - 1f;

                Vector3 randomOffset = new Vector3(noiseX, noiseY, 0f) * shakeAmount;
                cameraTransform.localPosition = cameraBaseLocalPos + randomOffset;
            }
            else
            {
                cameraTransform.localPosition = cameraBaseLocalPos;
            }
        }

        void ScaleAndLiftCard(Transform card, Vector3 startPos, bool lookingDown, bool isSelected, Image selector, float typingProgress)
        {
            float targetScale;

            if (!lookingDown)
            {
                targetScale = normalCardScale; // stari sistem - kartice nisu podignute, normalna veličina
            }
            else if (isSelected)
            {
                // podignuta + dodatno uvećana jer je selektovana + dodatno raste dok igrač kuca
                targetScale = enlargedCardScale + selectedExtraScale + typingBoostAmount * typingProgress;
            }
            else
            {
                // podignuta, ali nije selektovana - blago se smanjuje dok se selektovana kuca
                targetScale = enlargedCardScale - typingShrinkAmount * typingProgress;
            }

            card.localScale = Vector3.Lerp(
                card.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * cardScaleSpeed
            );

            Vector3 targetPos = startPos;
            targetPos.y = lookingDown ? startPos.y + liftOffsetY : startPos.y;

            card.localPosition = Vector3.Lerp(
                card.localPosition,
                targetPos,
                Time.deltaTime * liftSpeed
            );

            // Selektor iza kartice - fade in/out preko alpha kanala (boja ostaje ona koju si ti postavio u Inspectoru)
            if (selector != null)
            {
                float targetAlpha = (lookingDown && isSelected) ? 1f : 0f;
                Color currentColor = selector.color;
                float newAlpha = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * selectorFadeSpeed);

                selector.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            }
        }

        void PlayRandomScrollSound()
        {
            if (sfxSource == null || scrollSounds == null || scrollSounds.Length == 0) return;

            int randomIndex = Random.Range(0, scrollSounds.Length);
            sfxSource.PlayOneShot(scrollSounds[randomIndex]);
        }

        public bool AddCard(Sprite sprite, string word)
{
    if (cardCount >= maxCards) return false; // nema mesta

    TypingListener[] typings = { card1Typing, card2Typing, card3Typing, card4Typing };
    Transform[] cards = { card1, card2, card3, card4 };

    int slot = cardCount; // prvo prazno mesto: sa 2 kartice je to indeks 2 (kartica 3)

    Image img = cardImages[slot] != null ? cardImages[slot] : cards[slot].GetComponent<Image>();
    if (img != null) img.sprite = sprite;
    else Debug.LogWarning("Kartica " + (slot + 1) + " nema Image komponentu za sprite!");

    if (typings[slot] != null) typings[slot].SetWord(word);

    cardCount++;
    return true;
}

        void Update()
        {
            // Cards Scaling System
    float pitch = transform.localEulerAngles.x;
    if (pitch > 180f) pitch -= 360f;

    bool lookingDown = pitch >= lookDownThreshold;
    isLookingDown = lookingDown;

    // Cards Scroll System
    // WHEN CARDS ARE UP
    if (lookingDown)
    {
        if (Input.mouseScrollDelta.y > 0f)
        {
            selectedCardIndex = (selectedCardIndex - 1 + 4) % 4; // ide na sledeću, wrap-uje sa 3 na 0
            PlayRandomScrollSound();
        }
        else if (Input.mouseScrollDelta.y < 0f)
        {
            selectedCardIndex = (selectedCardIndex + 1) % 4; // ide na prethodnu, wrap-uje sa 0 na 3
            PlayRandomScrollSound();
        }
    }

    // Progres kucanja trenutno selektovane kartice (0 ako niko ne kuca ili nema reference)
    float selectedProgress = 0f;
    switch (selectedCardIndex)
    {
        case 0: if (card1Typing != null) selectedProgress = card1Typing.TypingProgress; break;
        case 1: if (card2Typing != null) selectedProgress = card2Typing.TypingProgress; break;
        case 2: if (card3Typing != null) selectedProgress = card3Typing.TypingProgress; break;
        case 3: if (card4Typing != null) selectedProgress = card4Typing.TypingProgress; break;
    }

    ScaleAndLiftCard(card1, card1StartPos, lookingDown, selectedCardIndex == 0, card1Selector, selectedProgress);
    ScaleAndLiftCard(card2, card2StartPos, lookingDown, selectedCardIndex == 1, card2Selector, selectedProgress);
    ScaleAndLiftCard(card3, card3StartPos, lookingDown, selectedCardIndex == 2, card3Selector, selectedProgress);
    ScaleAndLiftCard(card4, card4StartPos, lookingDown, selectedCardIndex == 3, card4Selector, selectedProgress);

    ApplyCameraShake(selectedProgress);


    //Cards Activation System

    // CARD 1
    if (lookingDown && selectedCardIndex == 0 && Input.GetMouseButtonDown(1))
    {
        Debug.Log("Aktivacija 1 kartice");
    }
            
        }
    }
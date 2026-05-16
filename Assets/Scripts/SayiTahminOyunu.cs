using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaslangicSeviye.SayiTahminOyunu
{
    /// <summary>
    /// 1-20 arasında rastgele sayı üreten ve kullanıcının tahminini kontrol eden oyun scripti.
    /// </summary>
    public class SayiTahminOyunu : MonoBehaviour
    {
        [Header("UI Referansları")]
        [SerializeField] private TMP_InputField guessInputField;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text remainingLivesText;
        [SerializeField] private Button guessButton;
        [SerializeField] private Button restartButton;

        [Header("Sayı Aralığı")]
        [SerializeField] private int minSayi = 1;
        [SerializeField] private int maxSayi = 20;
        [SerializeField] private int maksimumHak = 5;

        private int hedefSayi;
        private int kalanHak;
        private const float MesajVarsayilanBoyut = 32f;
        private const float MesajIpucuBoyut = 28f;

        // Modern tema renkleri
        private readonly Color mesajNormalRenk = new Color32(248, 250, 252, 255); // #F8FAFC
        private readonly Color mesajBasariRenk = new Color32(34, 197, 94, 255);   // #22C55E
        private readonly Color mesajHataRenk = new Color32(239, 68, 68, 255);      // #EF4444
        private const string BuyukRenkHex = "#3B82F6";
        private const string KucukRenkHex = "#F59E0B";

        private void Awake()
        {
            // Buton event'lerini kod üzerinden bağlıyoruz.
            if (guessButton != null)
            {
                guessButton.onClick.AddListener(TahminiKontrolEt);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(YenidenBaslat);
            }
        }

        private void Start()
        {
            YeniOyunBaslat();
        }

        private void OnDestroy()
        {
            // Olası memory leak'leri önlemek için event temizliği.
            if (guessButton != null)
            {
                guessButton.onClick.RemoveListener(TahminiKontrolEt);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(YenidenBaslat);
            }
        }

        /// <summary>
        /// Kullanıcının girdiği tahmini kontrol eder.
        /// </summary>
        private void TahminiKontrolEt()
        {
            if (guessInputField == null || messageText == null)
            {
                return;
            }

            string girilenMetin = guessInputField.text.Trim();

            // Harf, boşluk veya geçersiz format kontrolü.
            if (!int.TryParse(girilenMetin, out int oyuncuTahmini))
            {
                messageText.text = "Lütfen geçerli bir sayı gir.";
                messageText.color = mesajHataRenk;
                return;
            }

            // Aralık dışında girişleri de geçersiz say.
            if (oyuncuTahmini < minSayi || oyuncuTahmini > maxSayi)
            {
                messageText.text = "Lütfen geçerli bir sayı gir.";
                messageText.color = mesajHataRenk;
                return;
            }

            // Geçerli her tahminde bir hak azalır.
            kalanHak--;

            if (oyuncuTahmini < hedefSayi)
            {
                if (kalanHak <= 0)
                {
                    OyunuKaybet();
                }
                else
                {
                    messageText.text = $"İpucu: <color={BuyukRenkHex}>Daha büyük</color> bir sayı gir. Kalan hak: {kalanHak}";
                    messageText.color = mesajHataRenk;
                    messageText.fontSize = MesajIpucuBoyut;
                }
            }
            else if (oyuncuTahmini > hedefSayi)
            {
                if (kalanHak <= 0)
                {
                    OyunuKaybet();
                }
                else
                {
                    messageText.text = $"İpucu: <color={KucukRenkHex}>Daha küçük</color> bir sayı gir. Kalan hak: {kalanHak}";
                    messageText.color = mesajHataRenk;
                    messageText.fontSize = MesajIpucuBoyut;
                }
            }
            else
            {
                messageText.text = "Bildiniz!";
                messageText.color = mesajBasariRenk;
                messageText.fontSize = MesajVarsayilanBoyut;

                // Doğru tahminden sonra girişi ve tahmin butonunu kapat.
                guessInputField.interactable = false;
                guessButton.interactable = false;
            }

            // Sonraki deneme için input'u temizle.
            guessInputField.text = string.Empty;
            guessInputField.ActivateInputField();
            KalanHakMetniniGuncelle();
        }

        /// <summary>
        /// Oyunu sıfırlar ve yeni bir sayı üretir.
        /// </summary>
        private void YenidenBaslat()
        {
            YeniOyunBaslat();
        }

        /// <summary>
        /// Oyun başlangıç ayarlarını yapar.
        /// </summary>
        private void YeniOyunBaslat()
        {
            // int Random.Range üst sınırı dahil etmez; bu yüzden +1 kullanıyoruz.
            hedefSayi = Random.Range(minSayi, maxSayi + 1);
            kalanHak = Mathf.Max(1, maksimumHak);

            if (messageText != null)
            {
                messageText.text = "Tahmin bekleniyor...";
                messageText.color = mesajNormalRenk;
                messageText.fontSize = MesajVarsayilanBoyut;
            }

            if (guessInputField != null)
            {
                guessInputField.text = string.Empty;
                guessInputField.interactable = true;
                guessInputField.ActivateInputField();
            }

            if (guessButton != null)
            {
                guessButton.interactable = true;
            }

            KalanHakMetniniGuncelle();
        }

        /// <summary>
        /// Hak bittiğinde oyunu sonlandırır ve kaybetme mesajı gösterir.
        /// </summary>
        private void OyunuKaybet()
        {
            messageText.text = $"Kaybettin! Dogru sayi: {hedefSayi}";
            messageText.color = mesajHataRenk;
            messageText.fontSize = MesajVarsayilanBoyut;
            guessInputField.interactable = false;
            guessButton.interactable = false;
            KalanHakMetniniGuncelle();
        }

        /// <summary>
        /// Kalan hak bilgisini ayrı bir TMP metin alanında günceller.
        /// </summary>
        private void KalanHakMetniniGuncelle()
        {
            if (remainingLivesText == null)
            {
                return;
            }

            remainingLivesText.text = $"Kalan Hak: {kalanHak}";
            remainingLivesText.color = kalanHak <= 1 ? mesajHataRenk : mesajNormalRenk;
        }
    }
}

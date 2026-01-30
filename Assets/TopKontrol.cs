using UnityEngine;
using UnityEngine.SceneManagement;

public class TopKontrol : MonoBehaviour
{
    Rigidbody rb;
    
    // ŞUT AYARLARI
    [Header("Güç Ayarları")]
    public float gucCarpani = 15f;
    public float yukseklikCarpani = 3f;
    public float maksimumGuc = 2000f;
    public float maksimumCekmeMessafesi = 300f; // Pixel cinsinden - ne kadar çekebilir?
    
    [Header("Görsel Feedback")]
    public LineRenderer yonCizgisi; // Çekme yönünü gösterecek
    public GameObject gucGostergesi; // UI göstergesi (opsiyonel)
    public int trajektorNoktaSayisi = 30; // Yörünge tahmini için
    public float trajektorZamanAdimi = 0.1f;
    
    [Header("Ses Efektleri (Opsiyonel)")]
    public AudioSource sesKaynagi;
    public AudioClip sutSesi;
    public AudioClip golSesi;
    
    // OYUN DURUMU
    bool oyunBitti = false;
    bool nisanAliniyor = false; // Şu an çekiyor mu?
    Vector3 baslangicMousePozisyonu;
    Vector3 suankiMousePozisyonu;
    
    // Güç hesaplama için
    float mevcutGuc = 0f;
    float mevcutGucYuzdesi = 0f; // 0-1 arası

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // LineRenderer hazırlığı (yoksa uyarı ver)
        if (yonCizgisi == null)
        {
            Debug.LogWarning("LineRenderer atanmadı! Görsel feedback için eklemelisin.");
        }
        else
        {
            yonCizgisi.enabled = false;
            yonCizgisi.positionCount = trajektorNoktaSayisi;
        }
        
        if (gucGostergesi != null)
        {
            gucGostergesi.SetActive(false);
        }
    }

    void Update()
    {
        // Eğer oyun bittiyse veya top hareket halindeyse şut çektirmeyelim
        if (oyunBitti || rb.linearVelocity.magnitude > 1f)
        {
            if (nisanAliniyor)
            {
                NisanIptal(); // Hareket varken çizgileri kapat
            }
            return;
        }

        // ========== 1. MOUSE'A BASILDIĞI AN (Nişan Alma Başladı) ==========
        if (Input.GetMouseButtonDown(0))
        {
            baslangicMousePozisyonu = Input.mousePosition;
            nisanAliniyor = true;
            
            // Görsel feedback'leri aç
            if (yonCizgisi != null)
                yonCizgisi.enabled = true;
            
            if (gucGostergesi != null)
                gucGostergesi.SetActive(true);
        }

        // ========== 2. MOUSE BASILI TUTULURKEN (Çekiyor...) ==========
        if (Input.GetMouseButton(0) && nisanAliniyor)
        {
            suankiMousePozisyonu = Input.mousePosition;
            GucHesapla();
            YonCizgisiGuncelle();
        }

        // ========== 3. MOUSE BIRAKILDIĞI AN (Şut!) ==========
        if (Input.GetMouseButtonUp(0) && nisanAliniyor)
        {
            SutCek();
            NisanIptal();
        }

        // R tuşu acil durum reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            MaciTekrarla();
        }
    }

    void GucHesapla()
    {
        // Mouse'u ne kadar çektik?
        Vector3 cekmeVektoru = baslangicMousePozisyonu - suankiMousePozisyonu;
        float cekmeMessafesi = cekmeVektoru.magnitude;

        // Maksimum çekme mesafesini geçmesin (Angry Birds gibi sınır)
        if (cekmeMessafesi > maksimumCekmeMessafesi)
        {
            cekmeMessafesi = maksimumCekmeMessafesi;
        }

        // Güç yüzdesi (0-1 arası) - UI için
        mevcutGucYuzdesi = cekmeMessafesi / maksimumCekmeMessafesi;

        // Gerçek güç hesabı
        mevcutGuc = mevcutGucYuzdesi * maksimumGuc;

        // Debug için (konsola yazdır)
        // Debug.Log($"Çekme: {cekmeMessafesi:F0}px | Güç: %{mevcutGucYuzdesi * 100:F0}");
    }

    void YonCizgisiGuncelle()
    {
        if (yonCizgisi == null) return;

        // Çekme vektörü
        Vector3 cekmeVektoru = baslangicMousePozisyonu - suankiMousePozisyonu;
        
        // Maksimum mesafeyi sınırla
        if (cekmeVektoru.magnitude > maksimumCekmeMessafesi)
        {
            cekmeVektoru = cekmeVektoru.normalized * maksimumCekmeMessafesi;
        }

        // 2D ekran koordinatlarını 3D oyun dünyasına çevir
        Vector3 vurusYonu = new Vector3(cekmeVektoru.x, 0, cekmeVektoru.y);
        
        // Yükseklik ekle
        vurusYonu.y = cekmeVektoru.magnitude / yukseklikCarpani;
        
        // Kuvvet vektörü
        Vector3 kuvvet = vurusYonu * gucCarpani;

        // Trajektör tahmini çiz
        TrajektorCiz(rb.position, kuvvet / rb.mass); // Force'u velocity'ye çevir
    }

    void TrajektorCiz(Vector3 baslangicPos, Vector3 baslangicHiz)
    {
        Vector3 pozisyon = baslangicPos;
        Vector3 hiz = baslangicHiz;
        Vector3 yercekimi = Physics.gravity;

        for (int i = 0; i < trajektorNoktaSayisi; i++)
        {
            yonCizgisi.SetPosition(i, pozisyon);

            // Fizik simülasyonu (basit Euler integration)
            hiz += yercekimi * trajektorZamanAdimi;
            pozisyon += hiz * trajektorZamanAdimi;

            // Eğer yere çarptıysa durduralım
            if (pozisyon.y < 0)
            {
                // Kalan noktaları son pozisyona ayarla
                for (int j = i; j < trajektorNoktaSayisi; j++)
                {
                    yonCizgisi.SetPosition(j, pozisyon);
                }
                break;
            }
        }
    }

    void SutCek()
    {
        // Çekme vektörü
        Vector3 cekmeVektoru = baslangicMousePozisyonu - suankiMousePozisyonu;
        
        // Maksimum mesafeyi sınırla
        if (cekmeVektoru.magnitude > maksimumCekmeMessafesi)
        {
            cekmeVektoru = cekmeVektoru.normalized * maksimumCekmeMessafesi;
        }

        // Minimum çekme kontrolü (Çok az çekerse şut olmasın)
        if (cekmeVektoru.magnitude < 10f)
        {
            Debug.Log("Çok zayıf! Daha fazla çek.");
            return;
        }

        // 2D mouse hareketini 3D vuruş yönüne çevir
        Vector3 vurusYonu = new Vector3(cekmeVektoru.x, 0, cekmeVektoru.y);
        
        // Yükseklik ekle (Aşırtma için)
        vurusYonu.y = cekmeVektoru.magnitude / yukseklikCarpani;
        
        // Kuvvet hesapla
        Vector3 sonKuvvet = vurusYonu * gucCarpani;
        
        // Maksimum güç limiti (Güvenlik)
        if (sonKuvvet.magnitude > maksimumGuc)
        {
            sonKuvvet = sonKuvvet.normalized * maksimumGuc;
        }

        // Topa vur!
        rb.AddForce(sonKuvvet);

        // Ses efekti
        if (sesKaynagi != null && sutSesi != null)
        {
            sesKaynagi.PlayOneShot(sutSesi);
        }

        Debug.Log($"ŞUT! Güç: %{mevcutGucYuzdesi * 100:F0} | Kuvvet: {sonKuvvet.magnitude:F0}");
    }

    void NisanIptal()
    {
        nisanAliniyor = false;
        
        if (yonCizgisi != null)
            yonCizgisi.enabled = false;
        
        if (gucGostergesi != null)
            gucGostergesi.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (oyunBitti) return;

        // KALE İÇİ
        if (other.CompareTag("Kaleici"))
        {
            Debug.Log("GOOOOOLLL!!! ⚽");
            
            if (sesKaynagi != null && golSesi != null)
            {
                sesKaynagi.PlayOneShot(golSesi);
            }
            
            OyunBitir();
            Invoke("MaciTekrarla", 2.0f);
        }

        // DIŞARI (OUT)
        if (other.CompareTag("Disari"))
        {
            Debug.Log("Dışarı Gitti! (Out) ❌");
            OyunBitir();
            Invoke("MaciTekrarla", 1.0f);
        }
    }

    void OyunBitir()
    {
        oyunBitti = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void MaciTekrarla()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // GUI'de güç göstergesi (opsiyonel - test için)
    void OnGUI()
    {
        if (nisanAliniyor)
        {
            GUI.Label(new Rect(10, 10, 300, 30), 
                $"Güç: {mevcutGucYuzdesi * 100:F0}% | Kuvvet: {mevcutGuc:F0}");
        }
    }
}
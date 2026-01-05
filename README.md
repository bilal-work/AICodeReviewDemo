# AICodeReviewDemo

## 📌 Overview

**AICodeReviewDemo**, kurumsal yazılım projelerinde sıkça karşılaşılan  
**güvenlik açıklarını, kod kokularını ve sürdürülebilirlik problemlerini**  
yapay zekâ destekli kod inceleme araçları ile tespit etmeyi ve iyileştirmeyi amaçlayan
örnek bir **.NET Web API** projesidir.

Bu proje özellikle:

- Gerçek hayatta karşılaşılan **legacy (miras) kod problemlerini**
- AI destekli code review araçlarının **gerçek katkısını**
- SonarQube üzerinden **ölçülebilir kalite artışını**

göstermek için hazırlanmıştır.

---

## 🎯 Project Goals

Bu demo proje ile hedeflenenler:

- ❌ Klasik manuel code review süreçlerinin yetersizliklerini göstermek  
- ✅ Yapay zekâ destekli kod inceleme araçlarını gerçek senaryolarda kullanmak  
- ✅ SonarQube ile **nesnel (metric-based)** değerlendirme yapmak  
- ✅ Kod kalitesi, güvenlik ve sürdürülebilirliği birlikte ele almak  

> **Not:** Nihai değerlendirme **SonarQube Quality Gate** üzerinden yapılacak şekilde
tasarlanmıştır.

---

## 🧠 AI Code Review Yaklaşımı

Bu projede **tek bir araca bağlı kalınmamıştır**.  
Farklı katmanlarda çalışan araçlar birlikte kullanılmıştır:

### 🔹 Qodo (CodiumAI) – IDE Seviyesi
- Kod yazılırken:
  - Güvenlik açıklarını fark ettirir
  - Refactor önerileri sunar
  - Edge-case odaklı unit testler üretir
- Geliştirici üretkenliğini artırır

### 🔹 DeepCode (Snyk Code) – Güvenlik Odaklı AI
- AI tabanlı static analysis (SAST)
- SQL Injection, credential leak, sensitive data exposure gibi
  **gerçek güvenlik risklerini** yakalar
- CI/CD pipeline’da fail olacak şekilde konfigüre edilebilir

### 🔹 SonarQube – Nihai Hakem
- Code Smell
- Security Hotspot
- Maintainability
- Test Coverage
- Technical Debt

> **SonarQube bu projede “karar verici” roldedir.**

---

## 🏗️ Project Structure

```text
AICodeReviewDemo
│
├── Controllers
│   ├── LegacyUserController.cs
│   └── LegacyUserControllerTest.cs
│
├── Properties
│   └── launchSettings.json
│
├── appsettings.json
├── Program.cs
├── Dockerfile
├── AICodeReviewDemo.http
└── README.md


⚠️ LegacyUserController – Bilinçli Kötü Örnek

LegacyUserController, bilerek kurumsal hayatta sık karşılaşılan
problemli kodları içerecek şekilde yazılmıştır.

Bu controller’da yer alan problemler:

🔐 Hard-coded connection string ve şifre

💉 SQL Injection (string concatenation)

🔑 Plain-text password kullanımı

📤 Exception detaylarının kullanıcıya dönülmesi

📄 Sensitive bilgilerin (password) API response’unda yer alması

🧠 Paging olmadan tüm verinin RAM’e alınması

♻️ Dispose edilmeyen database connection’ları

🧪 Test edilebilirliği düşük tasarım

Bu yapı, AI destekli araçların neleri yakaladığını net şekilde göstermek
için özellikle tercih edilmiştir.

🧪 Unit Tests & Edge Cases

LegacyUserControllerTest sınıfı:

İş kuralı içeren CalculateRisk metodu için

AI tarafından üretilmiş ve zenginleştirilmiş
boundary / edge-case testlerini içerir

Bu testler sayesinde:

Refactor sonrası davranış bozulmaları engellenir

SonarQube test coverage metrikleri anlamlı hale gelir

🔍 SonarQube Değerlendirme Kapsamı

Bu proje SonarQube üzerinde aşağıdaki başlıklar üzerinden değerlendirilir:

✅ Security Hotspots

✅ SQL Injection Riskleri

✅ Hard-coded Secret Tespiti

✅ Code Smells

✅ Maintainability Index

✅ Test Coverage

✅ Quality Gate Durumu

🐳 Docker Support

Projede Dockerfile bulunmaktadır.

Amaç:

SonarQube

CI/CD

Lokal test ortamları

için kolay entegrasyon sağlamaktır.

docker build -t aicodereviewdemo .
docker run -p 8080:80 aicodereviewdemo

🚀 Getting Started
Prerequisites

.NET SDK 8 / 9

Docker (opsiyonel)

SonarQube (local veya remote)

Qodo (CodiumAI) IDE extension

Snyk account (DeepCode)

Run Locally
dotnet restore
dotnet build
dotnet run

📊 Expected Outcomes

Bu projeyi inceleyen bir kişi:

AI destekli code review’un nerede gerçekten fayda sağladığını

Legacy kodun nasıl ölçülebilir şekilde iyileştirilebileceğini

SonarQube Quality Gate kavramının neden kritik olduğunu

net şekilde görebilir.

📌 Target Audience

Software Engineers

Senior / Lead Developers

Software Architects

DevOps Engineers

Akademik değerlendirme yapan eğitmenler

AI destekli yazılım kalite süreçlerini inceleyen ekipler

🧭 Next Steps

Bu demo projenin devamında:

ModernUserController (best-practice versiyon)

Before / After SonarQube raporları

CI/CD pipeline entegrasyonu

Snyk Code fail eden build örnekleri

eklenebilir.

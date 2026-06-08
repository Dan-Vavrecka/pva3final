# 🎲 Hra: Hod kostkami (C# Windows Forms)

Aplikace představuje plně funkční desktopovou hru v kostky (známou také jako *Zonkey* nebo *10000*) naprogramovanou v jazyce **C#** pro rozhraní **Windows Forms (.NET)**. Projekt kombinuje dynamické řízení prvků uživatelského rozhraní s vektorovým vykreslováním grafiky.

---

## 🛠️ 1. Architektura & Struktura souborů

Projekt je striktně rozdělen podle standardů objektového návrhu Windows Forms:
* 📄 **`Form1.cs`** – Hlavní řídicí centrum aplikace. Obsahuje kompletní herní logiku, matematické vyhodnocování bodových kombinací, stavový automat pro střídání tahů (Hráč vs. Hráč / CPU) a nízkoúrovňové překreslování plátna.
* 📄 **`Form1.Designer.cs`** – Automaticky generovaný kód vizuálního návrháře Visual Studia, který spravuje rozvržení oken, tlačítek, číselníků a textových popisků.

---

## ⚙️ 2. Konfigurace herní partie

Před samotným spuštěním hry (kliknutím na tlačítko `Start hry`) má uživatel plnou kontrolu nad parametry zápasu:

| Parametr | Možnosti nastavení | Popis funkce |
| :--- | :--- | :--- |
| **Režim hry** | 1 Hráč (vs. CPU) / 2 Hráči | Určuje, zda bude druhý tah simulován umělou inteligencí, nebo dalším člověkem u jednoho PC. |
| **Počet kostek** | Volitelně **1 až 6 kostek** | Dynamicky mění počet kostek, se kterými se v dané partii hraje. |
| **Kritérium konce** | Limit kol **vs.** Cílové skóre | **Limit kol:** Hra končí po fixním počtu kol.<br>**Cílové skóre:** Vítězí ten, kdo dříve dosáhne hranice (např. 10 000 bodů). |

---

## 🏆 3. Bodový systém & Pravidla kombinací

Aplikace po každém hodu provede frekvenční analýzu padnutých čísel a vypočítá skóre podle následující hierarchie:

### 🌟 Speciální kombinace (Při hodu 6 kostkami)
1. **Čistá postupka `1-2-3-4-5-6`** $
ightarrow$ **1 500 bodů**
2. **Tři libovolné dvojice** (např. `2-2, 4-4, 5-5`) $
ightarrow$ **1 000 bodů**

### 📊 Opakující se hodnoty (Trojice a vícenásobky)
Základ tvoří **trojice stejných čísel**. Každá další kostka s touto hodnotou navíc předchozí zisk **zdvojnásobí**:

| Hodnota na kostce | Skóre za 3ks (Základ) | Skóre za 4ks | Skóre za 5ks | Skóre za 6ks |
| :---: | :---: | :---: | :---: | :---: |
| **1** ⚀ | **1 000 b** | 2 000 b | 4 000 b | 8 000 b |
| **2** ⚁ | **200 b** | 400 b | 800 b | 1 600 b |
| **3** ⚂ | **300 b** | 600 b | 1 200 b | 2 400 b |
| **4** ⚃ | **400 b** | 800 b | 1 600 b | 3 200 b |
| **5** ⚄ | **500 b** | 1 000 b | 2 000 b | 4 000 b |
| **6** ⚅ | **600 b** | 1 200 b | 2 400 b | 4 800 b |

### 🎯 Samostatné kostky
Pokud kostky netvoří žádnou z výše uvedených kombinací, bodují pouze tyto samostatné hodnoty:
* Samostatná **Jednička (1)** $
ightarrow$ **100 bodů** za kus
* Samostatná **Pětka (5)** $
ightarrow$ **50 bodů** za kus
* *(Ostatní čísla 2, 3, 4, 6 samostatně žádné body nepřinášejí)*

---

## 🎨 4. Pokročilé programátorské techniky v projektu

Tento projekt záměrně nepoužívá začátečnické postupy a díky tomu splňuje nároky na pokročilé školní či semestrální práce:

1. **Runtime generování prvků:** `PictureBoxy` reprezentující kostky nejsou napevno vytvořeny v Designeru. Vytvářejí se dynamicky v cyklu v metodě `OnLoad` na základě zvoleného nastavení.
2. **Vektorový subsystém `Paint`:** Kostky nevyužívají bitmapové obrázky z disku. Jsou kompletně vykreslovány za běhu pomocí metod `Graphics.Clear`, `DrawRectangle` a `FillEllipse`. To zajišťuje perfektní ostrost při jakémkoliv rozlišení obrazovky.
3. **Nízkoúrovňové bitové operace:** Výpočet násobků skóre (zdvojnásobování bodů) je realizován pomocí bitového posunu doleva: `1 << (c - 3)`. To eliminuje potřebu rozsáhlých vnořených podmínek `if-else`.

---

## 🚀 5. Návod k instalaci a spuštění

1. Otevřete **Visual Studio** a vytvořte nový projekt typu **Windows Forms App (.NET)**.
2. Pojmenujte projekt jako `WinFormsApp1`.
3. Nahraďte zdrojový kód v souborech `Form1.cs` a `Form1.Designer.cs` kódem z tohoto repozitáře.
4. Klikněte na tlačítko **Spustit (F5)**.

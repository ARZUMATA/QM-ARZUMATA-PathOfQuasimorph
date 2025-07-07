# QM_PathOfQuasimorph

<p align="center">
  <a href="readme.md">English</a> ·
  <a href="readme_ru.md">Русский</a>
</p>

Ever wondered why your items so bad?

Do you break down more gear than you use?

Is every loot drop a disappointment?

Do you yearn for the thrill of the perfect find, the rush of discovering a truly legendary weapon?

Or maybe yoe you an ARPG gamer who miss vibe for magic and rare items?

Well this is the mod for you!

**Then descend into the Path of Quasimorph, where every item holds the potential for greatness!**

### **Forge Your Legend with Path of Quasimorph!**

## Path of Quasimorph

![thumbnail icon](media/thumbnail.jpg)

## Image Gallery

<p>
    <a href="media/160618.jpg" data-sub-html="Description">
      <img alt="Description" src="media/160618.jpg" width="300" />
    </a>
</p>

<details>
  <summary style="color: #448aff; font-weight: bold;">
    <h3 style="margin: 0; font-size: 24px;"> Click for more</h3>
  </summary>

<p>
  <a href="media/160601.jpg" data-sub-html="160601">
    <img alt="160601" src="media/160601.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/160618.jpg" data-sub-html="160618">
    <img alt="160618" src="media/160618.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/162125.jpg" data-sub-html="162125">
    <img alt="162125" src="media/162125.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/162132.jpg" data-sub-html="162132">
    <img alt="162132" src="media/162132.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231200.jpg" data-sub-html="231200">
    <img alt="231200" src="media/231200.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231203.jpg" data-sub-html="231203">
    <img alt="231203" src="media/231203.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231206.jpg" data-sub-html="231206">
    <img alt="231206" src="media/231206.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231211.jpg" data-sub-html="231211">
    <img alt="231211" src="media/231211.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231220.jpg" data-sub-html="231220">
    <img alt="231220" src="media/231220.jpg" width="300" />
  </a>
</p>

</details>

## Features

* A New Loot Paradigm: Say goodbye to mundane loot! Path of Quasimorph introduces a dynamic rarity system, categorizing items into distinct tiers, each brimming with potential and its own amount of boosted properties and extra traits:

       - Standard: The baseline, nothing can surprise you.
       - Enhanced: A noticeable step up, with improved stats.
       - Advanced: Starting to show real power, with enhanced attributes.
       - Premium: Coveted finds, boasting significant stat boosts and the potential for more extra traits.
       - Prototype: Rare and powerful, these represent cutting-edge technology and devastating potential.
       - Quantum: The pinnacle of rarity! Discovering a Quantum item is a cause for celebration.
         Imbued - with incredible stats, unique traits, and even the possibility of unbreakable durability!

* Weighted Drops, Epic Finds: Just like the best ARPGs, rarity matters! Quantum items are incredibly rare, making each find a truly momentous occasion. You are lucky for sure!

* Visually Distinct Gear: Instantly identify your new possessions! Each rarity tier is distinguished by a unique background color, allowing you to quickly assess the value of every drop.

* Prefixes and Affixes: Each item comes with with custom prefixes and affixes, adding flavor to your most treasured gear and game. 

(This will be update with more prefixes based on boosted stats)

- **Customizable Configuration**: Allows users to adjust settings through a [Mod Configuration Menu (MCM).](https://steamcommunity.com/sharedfiles/filedetails/?id=3469678797)

## Requirements (Optional)

- **MCM (Mod Configuration Menu)**: A configuration menu framework to manage settings via an in-game interface.

As alternative you can find config files in:
- `%AppData%\..\LocalLow\Magnum Scriptum Ltd\Quasimorph_ModConfigs\QM_PathOfQuasimorph\config_mcm.ini`

# Configuration
| Name                  | Default | Description                                                                 |
|-----------------------|---------|-----------------------------------------------------------------------------|
| Enable                |true     | Enable or disable  mod. Stops generation new items. Keeping existing ones.  |
| CleanupMode           |false    | It will try to cleanup your save game in case you decide to unsubscribe.    |

CleanupMode works this way:
Once enabled, load game, be in "space" mode, it will try to cleanup all items made by mod making them "standard", save the game. Then you can keep playing without mod enabled and clean item pool.
(This will be improved)

# External files

| File             | Description                                 | Documentation |
|------------------|---------------------------------------------|---------------|
| Rarities.csv     | Contains rarity-related parameters for items. | [EN version](docs/Rarities_CSV_EN.md) / [RU version](docs/Rarities_CSV_RU.md) |

# Source Code
Source code is available on [GitHub](https://github.com/ARZUMATA/QM-ARZUMATA-PathOfQuasimorph)

Thanks to NBK_RedSpy, Crynano and all the people who make their code open source.

# Change Log
## 1.0 (be3c4c6)
- Первый выпуск

## 1.1 (8d85683)
- Исправлен баг, когда при начале новой игры, игра не работала.
- Некоторые файлы не загружались в воркшоп, из-за чего мод не работал.
- Обновлен манифест, чтобы он соответствовал бета версии, и не появлялся в списке модов для стабильной версии.

## 1.2 (92e9f12)
- Модификаторы теперь применяются лучше.
- Одна характеристика усиливается немного сильнее, что определяет суффикс вещи.
- Улучшены усиления резистов, чтобы использовать минимальные и максимальные значения для лучшей вариативности.
- Теперь броня использует среднее значение сопротивления, чтобы определить, какое сопротивление случайным образом добавляется при успешном ролле.
- Исправлена некорректная логика усиления.
- Улучшено ведение журнала отладки для упрощения отладки и отслеживания изменений.

## 1.3 (850ae94)
- Шанс неломаемой вещи теперь имеет взвешенное значение.
- Параметры для изменения теперь выбираются в процентном диапазоне, а не фиксированным процентом.
- Исправлена одна проблема, из-за которой некоторые предметы не обрабатывались и сбрасывались до ванильных. Наблюдаем.
- Подсказки для оружия для сравнения версии PoQ с ванильной. Горячая клавиша "левый шифт".
- CSV-файл для рарок и шансов теперь доступен для редактирования и заменяет внутренние данные. Смотрите настройки мода, чтобы включить. [Документация на GitHub](https://github.com/ARZUMATA/QM-ARZUMATA-PathOfQuasimorph/tree/main/docs)

## 1.4 (aef44c6)
- Исправлены внутренние расчеты RNG.
- Трейты для оружия дальнего боя и ближнего боя теперь имеют исключения для удаления уж совсем абсурдных трейтов.
- Тултипы для сопротивлений брони.
- Среднее сопротивление, если применяется, не такое уж сильное.
- Исправлена ошибка, что делало броню слишком тяжелой.
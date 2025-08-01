# QM_PathOfQuasimorph

<p align="center">
  <a href="readme.md">English</a> ·
  <a href="readme_ru.md">Русский</a>
</p>

Задумался, почему твои предметы так плохи?

Разбираешь больше снаряжения, чем используешь?

Каждый дроп — это разочарование как наша жизнь?

А может жаждешь острых ощущений, идеальную шмотку, кайфа от дропа реально легендарного оружия?

Или ты игрок в ARPG, которому не хватает атмосферы магических и редких предметов?

Что ж, это мод для тебя мой дорогой друг!

**Вступи на путь квазиморфа, где каждый предмет таит в себе потенциал поистине настоящего величия!**

### **Сделай из своего Перси, настоящкю легенду с Path of Quasimorph!**

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

* Новая парадигма лута: Скажи пока скудному луту! Мод водит динамическую систему редкости, распределяя предметы по различным уровням, каждый из которых полон потенциала и обладает своим количеством усиленных свойств и дополнительных черт:

      - Стандартный: Базовый уровень, ничто не сможет тебя удивить.
      - Улучшенный: Заметный шаг вперед, с улучшенными характеристиками.
      - Продвинутый: Начинает проявлять настоящую мощь, с улучшенными атрибутами.
      - Премиум: Желанная вещь, обладающая значительным усилением характеристик и потенциалом для дополнительных.
      - Прототип: Редкая и мощная,  представляет собой передовые технологии, которым "аналоговнет" и разрушительный потенциал.
      - Квантовый: Вершина редкости! Обнаружение квантового предмета — повод для празднования. Наделён - невероятными характеристиками, уникальными чертами и даже возможностью неразрушимой прочности!

* Взвешенный дроп, эпические находки: как и в лучших ARPG, редкость имеет значение! Квантовые предметы невероятно редки, что делает каждую находку поистине знаменательным событием. Тебе точно повезло!

* Визуально отличимое снаряжение: Каждый уровень редкости отличается уникальным цветом фона, что позволяет быстро оценить ценность каждогой вещи.

* Префиксы и аффиксы: Каждый предмет поставляется с настраиваемыми префиксами и аффиксами, добавляя уникальности вашему самому ценному снаряжению и игре.

- **Настраиваемая конфигурация**: Позволяет пользователям изменять настройки через [Mod Configuration Menu (MCM).](https://steamcommunity.com/sharedfiles/filedetails/?id=3469678797)

## Требования

- **MCM (Mod Configuration Menu)**: Фреймворк меню конфигурации для управления настройками через внутриигровой интерфейс.

В качестве альтернативы вы можете найти файлы конфигурации по адресу:
- `%AppData%\..\LocalLow\Magnum Scriptum Ltd\Quasimorph_ModConfigs\QM_PathOfQuasimorph\config_mcm.ini`

# Configuration
| Название                  | По умолчанию | Описание                                                                 |
|-----------------------|---------|-----------------------------------------------------------------------------|
| Включить                |true     | Включает или отключает мод. Останавливает генерацию новых предметов. Существующие остаются.  |
| Режим очистки           |false    | Попытается очистить вашу сохраненную игру, если вы решите отписаться.    |

Режим очистки работает так:
После включения загрузите игру, перейдите в режим "космоса", мод попытается очистить все предметы, созданные им, сделав их "стандартными", затем сохраните игру. После этого вы сможете продолжить играть без включенного мода и с очищенным набором предметов.
(Будет улучшено)

# Внешние файлы

| File             | Description                                 | Documentation |
|------------------|---------------------------------------------|---------------|
| Rarities.csv     | Содержит все параметры для генерации предметов. | [EN version](docs/Rarities_CSV_EN.md) / [RU version](docs/Rarities_CSV_RU.md) |

# Исходный код
Исходный код доступен на [GitHub](https://github.com/ARZUMATA/QM-ARZUMATA-PathOfQuasimorph)

Спасибо NBK_RedSpy, Crynano и всем, кто делает свой код открытым.

# Исходный код

Ну я упоролась тестировать это все, но если что, сообщайте.
Пока цель не ломать сейв.
В любом случае, сделать копию сейва, определенно не помешает.

# Планы

Я хотела бы квартиру купить...

# Журнал изменений
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

## 1.41 (d7997d6)
- Исправлено: Для некоторых игроков характеристики модифицированных предметов были слишком экстремальными из-за региональных настроек и применялись неправильно, что приводило не только к визуальной ошибке, но и к слишком сильных характеристик предметов.
- Исправлено: Некоторые статы предметов сбрасывались.
- Префиксы предметов были обновлены, так что теперь префикс брони/оружия может определять его тип.
Например, «Перезаряженный» для оружия с электрическим уроном, «Крио» для холода, «Токсичный» для оружия с ядом, а также «Прочный» для брони. И т. д.
Спасибо сообществу за отзывы.

## 1.42 (1d5fcea)
- Исправлено: Некоторые предметы становились обычными. Да, надо было кое-где проверить.
- Исправлено: В некоторых случаях предметы появлялись в списке проектов, как будто они были разблокированы.
- Добавлено: Занижение параметров, теперь есть вероятность, что параметр будет уменьшен, а не увеличен, как обычно.
- Исправлено: Незначительные проблемы с форматированием подсказок.

## 1.43 (80a01dc)
- Исправлено: Вопрос баланса. Созданные предметы которые являются проектом (желтая М) не будут получать характеристики редкости.

## 1.5 (f9540b0)
- Улучшено ведение журнала с учётом журналов, специфичных для классов.
- Улучшена подсказка для сравнения предметов с дополнительной информацией.
- Генерация мобов теперь учитывает редкость и мастерство, включая дополнительное повышение характеристик предметов в зависимости от мастерства монстра.
- Исправлена проблема, из-за которой файлы использовались другими приложениями.
- Добавлен `MonsterInspectWindow` с поддержкой перков, подсказок и уровня мастера мобов.
- Добавлен новый спрайт класса для окна инспектора мобов.
- Исправлены проблемы десериализации и обеспечено правильное применение характеристик мобов.
- Исправлено некорректное отображение дополнительной информации в подсказке для обычных мобов.
- Исправлено повторное использование идентификаторов при генерации мобов.
- Добавлена поддержка рангов и талантов мобов.
- Добавлена поддержка перевода для нового контента.
- Добавлена возможность переключения генерации мобов.
- Обновлены вспомогательные функции и логгер для повышения производительности.
- Обновлены ресурсы Unity.

## 1.6-pre (7935d11)
- Рефактор системы создания предметов, теперь "внутренние" магнум проекты не нужны
- Добавлен `MobModifier` вещи так-же получают буст статов в зависимости от редкости мобов
- Поддержка **аугметаций**, **имплатов**
- Исправлен тултип, если предмет нельзя сломать - об этом сказано
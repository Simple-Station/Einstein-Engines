thief-role-greeting-human =
    Вы - преступное отродье, клептоман, ранее
    судимый за мелкую кражу, и досрочно освобождённый.
    Вам необходимо пополнить свою коллекцию.
thief-role-greeting-animal =
    Вы - клептоманящее животное.
    Воруйте всё, что вам нравится.
thief-role-greeting-equipment =
    У вас есть сумка воровских инструментов
    и воровские перчатки-хамелеоны.
    Выберите стартовое снаряжение,
    и незаметно делайте свою работу.
objective-issuer-thief = [color=#746694]Преступник[/color]
thief-round-end-agent-name = вор

### armsDealer
arms-dealer-round-end-agent-name = [color=crimson][bold]торговец оружием[/bold][/color]
roles-antag-arms-dealer-name = Торговец оружием
roles-antag-arms-dealer-objective = Используя свои контакты, контрабандой ввозите оружие на станцию и зарабатывайте деньги на его продаже.
role-subtype-arms-dealer = торговец оружием
arms-dealer-role-greeting-human = Вы - торговец оружием, ранее судимый за ваши преступления против мира и выпущенный после сделки по обмену заключёнными. Вам нужно заработать денег.
arms-dealer-role-greeting-equipment = У вас есть способность, позволяющая вам доставать оружие из любого закрытого ящика. Используйте её для получения товара.
container-summonable-summon-popup = Вы находите свою доставку в потайном отсеке {$target}.
ent-Orehum_ActionSummonGun = Доставка оружия
    .desc = Используйте свои контакты чтобы получить [color=crimson][bold]доставку оружия[/bold][/color] в любой закрытый контейнер.
admin-verb-make-arms-dealer = сделать торговцем оружием
steal-target-groups-cash = кредитов
objective-condition-arms-dealer-multiply-description = Мне нужно заработать { $count } { $itemName } и увезти их с собой.
objective-condition-earn-title = Заработать { $itemName }
ent-BaseArmsDealerObjective = { ent-BaseObjective }
    .desc = { ent-BaseObjective.desc }
ent-ArmsDealerMoneyObjective = { ent-BaseArmsDealerObjective }
    .desc = { ent-BaseArmsDealerObjective.desc }
ent-ArmsDealerGunFreeObjective = Продайте оружия
    .desc = Нужно продать как можно больше оружия. Неважно кому.

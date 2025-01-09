# Popups
blob-target-normal-blob-invalid = Неподходящий тип блоба.
blob-target-factory-blob-invalid = Неподходящий тип блоба, необходимо выбрать фабрику.
blob-target-node-blob-invalid = Неподходящий тип блоба, необходимо выбрать узел.
blob-target-nearby-not-node = Рядом нету узла или ядра.
blob-target-close-to-tile = Слишком близко к другой клетке данного типа.
blob-target-already-produce-blobbernaut = Данная фабрика уже произвела блоббернаута.
blob-target-already-connected = К узлу уже привязан блоб данного типа.
blob-cant-split = Вы не можете разделить ядро.
blob-not-enough-resources = Не хватает { $point } ресурсов для действия.
blob-mob-attack-blob = Вы не можете атаковать блоба.
blob-get-resource = +{ $point }
blob-spent-resource = -{ $point }
blobberaut-not-on-blob-tile = Вы умираете без тайлов блоба под ногами.
blobberaut-factory-destroy = Ваша фабрика была разрушена, вы умираете.
carrier-blob-alert = У вас осталось { $second } секунд до превращения.
blob-mob-zombify-second-start = { $pod } начинает превращать вас в зомби!
blob-mob-zombify-third-start = { $pod } начинает превращать { $target } в зомби!
blob-mob-zombify-second-end = { $pod } превращает вас в зомби!
blob-mob-zombify-third-end = { $pod } превращает { $target } в зомби!
blob-core-under-attack = Ваше ядро находится под атакой!

# UI
blob-chem-swap-ui-window-name = Смена химиката
blob-chem-reactivespines-info =
    Реактивные шипы
    Наносит 25 единиц брут урона.
blob-chem-blazingoil-info =
    Пылающее масло
    Наносит 15 урона ожогами и поджигает цели.
    Делает вас уязвимым к воде.
blob-chem-regenerativemateria-info =
    Регенеративная Материя
    Наносит 15 единиц урона ядами.
    Ядро востанавливает здоровье в 10 раз быстрее и дает на 1 очко больше.
blob-chem-explosivelattice-info =
    Взрывная решетка
    Наносит 5 единиц урона ожогами и взрывает цель, нанося 10 брут урона.
    Споры при смерти взрываются.
    Вы получаете имунитет к взрывам.
    Вы получаете на 50% больше урона ожогами и электричеством.
blob-chem-electromagneticweb-info =
    Электромагнитная паутина
    Наносит 20 урона ожогами, 20% шанс вызывать ЭМИ разряд при атаке.
    Любая уничтоженая плитка гарантировано вызовет ЭМИ.
    Вы получаете на 25% больше урона теплом и брутом.

blob-alert-out-off-station = Блоб был удален т.к. был обнаружен вне станции!

# game-rule

blob-title = Блоб
blob-description = Показатели биобезопасности станции указывают на наличие биологической угрозы 5-го уровня.

# Announcment
blob-alert-recall-shuttle = Эвакуационный шаттл не может быть отправлен на станцию пока существует биологическая угроза 5 уровня.
blob-alert-detect = На станции была обнаружена биологическая угроза 5 уровня, объявлена изоляция станции.
blob-alert-critical = Биологическая угроза достигла критической массы, вам отправлены коды от ядерной боеголовки, вы должны немедленно взорвать станцию.

# Actions
blob-create-factory-action-name = Создать блоб фабрику (40)
blob-create-factory-action-desc = Превращает выбраного нормального блоба в фабрику, которая способна производить различных преспешников блоба, если рядом есть узел или ядро.
blob-create-storage-action-name = Создать блоб хранилище (50)
blob-create-storage-action-desc = Превращает выбраного нормального блоба в хранилище, которое расширяет максимальное количество ресурсов которое может иметь блоб. 
blob-create-turret-action-name = Создать блоб турель (75)
blob-create-turret-action-desc = Превращает выбраного нормального блоба в турель, которая стреляет по врагам своими мелкими спорами, расходуя очки.
blob-create-resource-action-name = Создать ресурсный блоб (60)
blob-create-resource-action-desc = Превращает выбраного нормального блоба в ресурсного блоба который будет производить ресурсы если рядом есть узлы или ядро.
blob-produce-blobbernaut-action-name = Произвести блоббернаута на фабрике (60)
blob-produce-blobbernaut-action-desc = Производит на выбраной фабрике единожды блоббернаута который будет получать урон вне тайлов блоба и лечиться рядом с узлами.
blob-split-core-action-name = Разделить ядро (400)
blob-split-core-action-desc = Единоразово позволяет превратить выбраный узел в самостоятельное ядро которое будет развиваться независимо от вас.
blob-swap-core-action-name = Переместить ядро (200)
blob-swap-core-action-desc = Производит рокировку вашего ядра с выбраным узлом.
blob-teleport-to-core-action-name = Телепортироваться к ядру
blob-teleport-to-core-action-desc = Телепортирует вашу камеру к вашему ядру.
blob-create-node-action-name = Создать блоб узел (50)
blob-create-node-action-desc =
    Превращает выбраного нормального блоба в блоб узел.
    Узел будет активировать эфекты других блобов, лечить и расширяться в пределах своего действия уничтожая стены и создавая нормальные блобы.
blob-downgrade-action-name = Сбросить блоба (0)
blob-downgrade-action-desc = Превращает выбранную клетку обратно в обычного блоба для установки других видов клеток.
blob-swap-chem-action-name = Сменить химикат блоба (70)
blob-swap-chem-action-desc = Позволяет вам сменить текущий химикат на один из 4 выбранных.
blob-carrier-transform-to-blob-action-name = Превратиться в блоба
blob-carrier-transform-to-blob-action-desc = Мгновенно разрывает ваше тело и создает ядро блоба. Учтите что если под вами не будет тайлов - вы исчезнете.
blob-no-using-guns-popup = Большая палка?! БИТЬ!

# Ghost role
blob-carrier-role-name = Носитель блоба
blob-carrier-role-desc = Сущность зараженная "блобом".
blob-carrier-role-rules =
    Вы антагонист. У вас есть 4 минуты перед тем как вы превратитесь в блоба.
    Найдите за это время укромное место для стартовой точки заражения станции, ведь вы очень слабы в первые минуты после создания ядра.
blob-carrier-role-greeting = Вы носитель Блоба. Найдите укромное место на станции и превратитесь в Блоба. Превратите станцию в массу, а ее обитателей в ваших слуг. Все мы Блоб.

# Verbs
blob-pod-verb-zombify = Зомбировать
blob-verb-upgrade-to-strong = Улучшить до сильного блоба
blob-verb-upgrade-to-reflective = Улучшить до отражающего блоба
blob-verb-remove-blob-tile = Убрать блоба

# Alerts
blob-resource-alert-name = Ресурсы ядра
blob-resource-alert-desc = Ваши ресурсы которые производят ресурсные блобы и само ядро, требуются для разрастания и особых блобов.
blob-health-alert-name = Здоровье ядра
blob-health-alert-desc = Здоровье вашего ядра. Если оно опустится до 0 вы умрёте.

# Greeting
blob-role-greeting =
    Вы блоб - космический паразит который захватывает станции.
    Ваша цель - стать как можно больше не дав себя уничтожить.
    Используйте горячие клавиши Alt+LMB чтобы улучшать обычные плитки до сильных а сильные до отражающих.
    Позаботьтесь о получении ресурсов с блобов ресурсов.
    Вы практически неуязвимы к физическим повреждениям, но опасайтесь теплового урона.
    Учтите что особые клетки блоба работают только возле узлов или ядра.
blob-zombie-greeting = Вы были заражены спорой блоба которая вас воскресила, теперь вы действуете в интересах блоба.

# End round
blob-round-end-result =
    { $blobCount ->
        [one] Был один блоб.
       *[other] Было { $blobCount } блобов.
    }
blob-user-was-a-blob = [color=gray]{ $user }[/color] был блобом.
blob-user-was-a-blob-named = [color=White]{ $name }[/color] ([color=gray]{ $user }[/color]) был блобом.
blob-was-a-blob-named = [color=White]{ $name }[/color] был блобом.
preset-blob-objective-issuer-blob = [color=#33cc00]Блоб[/color]
blob-user-was-a-blob-with-objectives = [color=gray]{ $user }[/color] был блобом:
blob-user-was-a-blob-with-objectives-named = [color=White]{ $name }[/color] ([color=gray]{ $user }[/color]) был блобом:
blob-was-a-blob-with-objectives-named = [color=White]{ $name }[/color] был блобом:
blob-objective-percentage = Он захватил [color=White]{ $progress }%[/color] до победы.
blob-end-victory = [color=Red]Блоб(ы) успешно поглотил станцию![/color]
blob-end-fail = [color=Green]Блоб(ы) не смог поглотить станцию.[/color]
blob-end-fail-progress = Суммарно было захвачено [color=Yellow]{ $progress }%[/color] до победы.

# Objectivies
objective-condition-blob-capture-title = Захватить станцию
objective-condition-blob-capture-description = Ваша единственная цель - полное и безоговорочное поглощение станции. Вам необходимо владеть как минимум { $count } тайлами блоба.
objective-condition-success = { $condition } | [color={ $markupColor }]Успех![/color]
objective-condition-fail = { $condition } | [color={ $markupColor }]Провал![/color] ({ $progress }%)

lathe-menu-title = Меню станка
lathe-menu-queue = Очередь
lathe-menu-server-list = Список серверов
lathe-menu-sync = Синхр.
lathe-menu-search-designs = Поиск проектов
lathe-menu-category-all = Все
lathe-menu-search-filter = Фильтр
lathe-menu-amount = Кол-во:
lathe-menu-material-display = { $material } ({ $amount })
lathe-menu-tooltip-display = {$amount} {$material}
lathe-menu-description-display = [italic]{$description}[/italic]
lathe-menu-material-amount = { $amount ->
    [1] {NATURALFIXED($amount, 2)} {$unit}
    *[other] {NATURALFIXED($amount, 2)} {$unit}
}
lathe-menu-material-amount-missing = { $amount ->
    [1] {NATURALFIXED($amount, 1)} {$unit} {$material}, [color=red]требуется {NATURALFIXED($missingAmount, 1)} {$unit}[/color]
    *[other] {NATURALFIXED($amount, 1)} {$unit} {$material}, [color=red]требуется {NATURALFIXED($missingAmount, 1)} {$unit}[/color]
}
lathe-menu-no-materials-message = Материалы не загружены
lathe-menu-connected-to-silo-message = Подключен к хранилищу ресурсов.
lathe-menu-fabricating-message = Создаем...
lathe-menu-materials-title = Материалы
lathe-menu-queue-title = Очередь создания

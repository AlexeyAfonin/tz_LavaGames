# tz_LavaGames
 
## Управление:
+ ЛКМ в любую точку экрана - игровой персонаж пойдет в эту точку
+ ПКМ в любой точке экрана:
  + Если игрок на башне - Произведется выстрел в указанную нажатую точку (При зажатии будут производиться беcпрерывные выстрелы с указанным интервалом)
  + Если игрок не на башне - Выведется сообщение, что нужно подняться на башню (Определение находится ли игрок на башне или нет, осуществляется проверкой нужного слоя под игроком)

## Вражеский моб
Вражеский моб может перемещаться в одну из 4х точек (WayPoint) в случайном порядке. 
У врага имеется радиус видимости (зрения). Если игрок попадет в этот радиус, то моб начнет сразу двигаться в сторону игрока, прервав свой предыдущий маршрут.

## Примечания
+ Игрок и вражеский моб имеют анимации. При изменении скорости передвижения кого-либо, анимация ходьбы, сменится, на анимацию бега.
+ Модельки игрока и вражеского моба брались из бесплатных ассетов [unity assets store](https://assetstore.unity.com)
+ Анимации вражеского моба предоставлялись вместе с [бесплатным ассетом](https://assetstore.unity.com/packages/3d/characters/humanoids/zombie-30232)
+ Анимации для игрока брались с сайта [Mixamo.com](https://www.mixamo.com)

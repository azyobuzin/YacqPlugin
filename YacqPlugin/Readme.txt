﻿YACQ Plugin
作者：@azyobuzin
Special Thanks：@takeshik
動作環境：Krile 2.4.1くらい

YACQ(http://www.yacq.net/)スクリプトを実行するKrile用プラグインです。
IPlugin.Loadedされたときにコンパイル・実行します。

★使い方
	1. pluginsフォルダにdll類を突っ込みます
	2. pluginsフォルダに「yacq_lib」というフォルダを作って
	   スクリプト（.yacqファイル）を投げ込みます
	3. Krileを起動すれば動き出します

★コンソール
	Krileの右上のメニューから[プラグイン(P)]→[YACQ コンソール]をクリックすると
	その場でスクリプトを実行できます。

	rc.yacqファイルを用意するとコンソール起動時に発動します。

★YACQによるクエリ
	検索ボックスで「q:y:"YACQのコード"」と記述するとYACQでTLをフィルターできます。

★ShowMessageBoxSample.yacq
	Krile起動時に「YACQ Plugin!!」とメッセージボックスを表示するだけの
	サンプルスクリプトです。
	うざいので削除することをお勧めします。

★SearchAndPost.yacq
	SearchAndPostPlugin(https://github.com/azyobuzin/SearchAndPostPlugin)
	をYACQで作り直しました。
	「Google:～」や「Wikipedia:～」などとツイートすると勝手にブラウザが起動します。
	Google, Wikipedia, MSDNに対応していますがスクリプトを書き換えることで
	好きな検索サービスを追加できます。

今後の予定
	・OfficialMediaUploadPlugin(https://github.com/azyobuzin/OfficialMediaUploadPlugin)
	　をリメイク
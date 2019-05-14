namespace WsMusicStore

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/demo">] Demo
    | [<EndPoint "/about">] About

module Templating =
    open WebSharper.UI.Html

    type MainTemplate = Templating.Template<"Main.html">

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             li [if endpoint = act then yield attr.``class`` "active"] [
                a [attr.href (ctx.Link act)] [text txt]
             ]
        [
            "Home" => EndPoint.Home
            "Demo" => EndPoint.Demo
            "About" => EndPoint.About
        ]

    let Main ctx action (title: string) (body: Doc list) =
        Content.Page(
            MainTemplate()
                .Title(title)
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )

module Site =
    open WebSharper.UI.Html

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            h1 [] [text "Home page"]
            p [] [a [attr.href (ctx.Link EndPoint.Demo)] [text "Demo"]]
            p [] [a [attr.href (ctx.Link EndPoint.About)] [text "About"]]
        ]

    let DemoPage ctx =
        Templating.Main ctx EndPoint.Demo "Demo" [
            h1 [] [text "Say Hi to the server!"]
            div [] [client <@ Client.Main() @>]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1 [] [text "About"]
            p [] [text "This is a template WebSharper client-server application."]
            p [] [a [attr.href (ctx.Link EndPoint.Home)] [text "Back to Home page."]]
        ]

    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.Demo -> DemoPage ctx
            | EndPoint.About -> AboutPage ctx
        )

    open WebSharper.Suave
    open Suave.Web

    /// Add here any other Suave WebParts that need to be served, besides WebSharper.
    let suaveSite = Suave.Files.browseHome

    do
        let rootDir = System.IO.Path.GetFullPath "../../.."
        startWebServer
            { defaultConfig with homeFolder = Some rootDir }
            (WebSharperAdapter.ToWebPart(Main, RootDirectory = rootDir, Continuation = suaveSite))

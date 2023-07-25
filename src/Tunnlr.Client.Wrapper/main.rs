fn main() -> wry::Result<()> {
    use wry::{
        application::{
            event::{Event, StartCause, WindowEvent},
            event_loop::{ControlFlow, EventLoop},
            window::WindowBuilder,
        },
        webview::WebViewBuilder,
    };

    enum UserEvent {
        NewWindow(String),
    }

    let event_loop: EventLoop<UserEvent> = EventLoop::with_user_event();
    let proxy = event_loop.create_proxy();
    let window = WindowBuilder::new()
        .with_title("Tunnlr")
        .with_maximized(true)
        .build(&event_loop)?;

    print!("Hi");
    let _webview = WebViewBuilder::new(window)?
        .with_url("http://127.0.0.1:5109")?
        .with_new_window_req_handler(move |uri: String| {
            let submitted = proxy.send_event(UserEvent::NewWindow(uri.clone())).is_ok();

            if submitted {
                let _ = open::that(uri);
            }

            // prevent opening the wry browser
            return false;
        })
        .build()?;

    event_loop.run(move |event, _, control_flow| {
        *control_flow = ControlFlow::Wait;

        match event {
            Event::NewEvents(StartCause::Init) => println!("Wry has started!"),
            Event::WindowEvent {
                event: WindowEvent::CloseRequested,
                ..
            } => *control_flow = ControlFlow::Exit,
            _ => (),
        }
    });
}

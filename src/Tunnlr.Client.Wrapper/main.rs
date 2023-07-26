use std::{path::PathBuf, fs, process::{Command}};

use include_dir::{include_dir, Dir};

static ASSETS: Dir = include_dir!("$CARGO_MANIFEST_DIR/assets");
static MAIN_APPLICATION: &str = "Tunnlr.Client.Web";


fn main() -> wry::Result<()> {
    let temp_dir: PathBuf = std::env::temp_dir().join("Tunnlr");
    let _ = fs::create_dir(temp_dir.clone());
    let _ = ASSETS.extract(temp_dir.clone());

    // Start main application
    let mut command = Command::new(temp_dir.clone().join(MAIN_APPLICATION));
    command.current_dir(temp_dir.clone());
    let mut process = command.spawn().unwrap();

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
            } => {
                *control_flow = ControlFlow::Exit;

                let _ = process.kill();
            },
            _ => (),
        }
    });
}
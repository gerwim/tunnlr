use std::{fs, path::PathBuf, process::Command};

#[cfg(target_os = "windows")]
use std::os::windows::process::CommandExt;

use include_dir::{include_dir, Dir};

static ASSETS: Dir = include_dir!("$CARGO_MANIFEST_DIR/assets");
static MAIN_APPLICATION: &str = "Tunnlr.Client.Web";

#[tokio::main]
async fn main() -> wry::Result<()> {
    let mut tunnlr_process;

    if cfg!(not(target_os = "macos")) {
        let temp_dir: PathBuf = std::env::temp_dir().join("Tunnlr");
        let _ = fs::create_dir(temp_dir.clone());
        let _ = ASSETS.extract(temp_dir.clone());

        // Start main application
        let mut command = Command::new(temp_dir.clone().join(MAIN_APPLICATION));
        #[cfg(target_os = "windows")] {
            const DETACHED_PROCESS: u32 = 0x00000008;
            command.creation_flags(DETACHED_PROCESS);
        }

        command.current_dir(temp_dir.clone());
        tunnlr_process = command.spawn().unwrap();
    } else {
        // Start main application
        let exec = std::env::current_exe()?;
        let executable_dir = exec.parent().expect("Should be a directory");

        let mut command = Command::new(executable_dir.clone().join(MAIN_APPLICATION));
        command.current_dir(executable_dir.clone());
        tunnlr_process = command.spawn().unwrap();
    }

    wait_for_tunnlr_start().await;

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
        .with_url("http://localhost:5109")?
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
            Event::NewEvents(StartCause::Init) => println!("Tunnlr has started!"),
            Event::WindowEvent {
                event: WindowEvent::CloseRequested,
                ..
            } => {
                *control_flow = ControlFlow::Exit;

                let _ = tunnlr_process.kill();
            }
            _ => (),
        }
    });
}

async fn wait_for_tunnlr_start() {
    let http_client = reqwest::Client::new();
    let mut is_ready = false;
    while !is_ready {
        let response = http_client.head("http://localhost:5109").send().await;

        if response.is_ok() && response.unwrap().status() == reqwest::StatusCode::OK {
            is_ready = true;
        }

        // Wait another round
        std::thread::sleep(std::time::Duration::from_millis(100));
    }
}

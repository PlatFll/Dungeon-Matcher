from pathlib import Path
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from functools import partial
ROOT=Path(__file__).resolve().parents[1]
ThreadingHTTPServer(("127.0.0.1",8892),partial(SimpleHTTPRequestHandler,directory=str(ROOT))).serve_forever()


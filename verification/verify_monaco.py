import time
from playwright.sync_api import sync_playwright

def verify_monaco():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()

        page.on("console", lambda msg: print(f"Console: {msg.text}"))
        page.on("pageerror", lambda exc: print(f"PageError: {exc}"))

        print("Navigating...")
        page.goto("http://localhost:5280")

        print("Waiting for body...")
        page.wait_for_selector("body")

        print("Waiting for container...")
        page.wait_for_selector("#container")

        print("Waiting for monaco-editor...")
        try:
            # Wait for any element with class monaco-editor
            page.wait_for_selector(".monaco-editor", timeout=10000)
            print("Found .monaco-editor")
        except Exception as e:
            print(f"Error finding .monaco-editor: {e}")
            return

        # Click into the editor to focus
        page.click(".monaco-editor")

        # Type some text to trigger completion
        page.keyboard.press("Control+End")
        page.keyboard.press("Enter")
        page.keyboard.type("Console.")

        # Wait for the suggestion widget to appear
        try:
            page.wait_for_selector(".suggest-widget.visible", timeout=10000)
            print("Suggestion widget appeared.")
        except Exception as e:
            print("Suggestion widget did not appear or timed out.")

        # Take a screenshot
        page.screenshot(path="verification/monaco_completion.png")

        browser.close()

if __name__ == "__main__":
    verify_monaco()

from playwright.sync_api import sync_playwright

def verify_frontend():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()

        try:
            # Go to the sample page
            page.goto("http://localhost:5280")

            # Wait for the editor to load
            page.wait_for_selector(".monaco-editor")

            # 1. Verify Status Bar
            # Check if status bar items are visible
            cursor_pos = page.locator("#cursor-position")
            validation_status = page.locator("#validation-status")

            if not cursor_pos.is_visible():
                print("Cursor position not visible")
            if not validation_status.is_visible():
                print("Validation status not visible")

            # 2. Verify Theme Toggle
            toggle_btn = page.locator("#theme-toggle")
            toggle_btn.click()

            # Wait for theme change (check body class)
            page.wait_for_selector("body.dark-theme")

            # Take screenshot of dark mode
            page.screenshot(path="/home/jules/verification/dark_mode.png")
            print("Taken screenshot of dark mode")

            # Toggle back
            toggle_btn.click()
            page.wait_for_selector("body:not(.dark-theme)")

            # 3. Simulate Loading state (manually trigger if possible or just check element existence)
            # The loading indicator is hidden by default.
            # We can try to execute JS to show it for verification
            page.evaluate("document.getElementById('loading-indicator').style.display = 'inline-block'")
            page.screenshot(path="/home/jules/verification/loading_state.png")
            print("Taken screenshot of loading state")

            # 4. Trigger validation (by waiting - the code has a delay)
            # The default code has "Guid." which is invalid syntax or might trigger completion.
            # Wait for validation status to update?
            # It might take some time for the backend to respond.

            page.wait_for_timeout(2000) # Wait for initial validation
            page.screenshot(path="/home/jules/verification/initial_state.png")
            print("Taken screenshot of initial state")

        except Exception as e:
            print(f"Error: {e}")
        finally:
            browser.close()

if __name__ == "__main__":
    verify_frontend()


def find_button(unium, text):
    return unium.wait_for_one(
        "/*.Button/*[Text.text='%s']/.." % (text,))

def test_cube_scene(unium):
    unium.wait_for_scene('MainScene', timeout=10)
    btn = find_button(unium, "CUBEMAINSCENE")
    btn.click()
    unium.wait_for_scene('CubeMainScene')
    unium.save_screenshot('cube_scene.png')

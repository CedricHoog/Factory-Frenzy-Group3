package com.factoryfrenzy.levelsave;

import android.app.Activity;
import android.content.ActivityNotFoundException;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.widget.Toast;

import java.io.IOException;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;

public final class LevelJsonSaveActivity extends Activity {
    private static final String TAG = "FactoryFrenzyLevelSave";
    private static final String EXTRA_JSON = "level_json";
    private static final String EXTRA_FILE_NAME = "level_file_name";
    private static final int REQUEST_CREATE_DOCUMENT = 7314;

    public static void openSavePicker(Activity activity, String json, String fileName) {
        Intent intent = new Intent(activity, LevelJsonSaveActivity.class);
        intent.putExtra(EXTRA_JSON, json);
        intent.putExtra(EXTRA_FILE_NAME, fileName);
        activity.startActivity(intent);
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        String json = getIntent().getStringExtra(EXTRA_JSON);
        String fileName = getIntent().getStringExtra(EXTRA_FILE_NAME);
        if (json == null || fileName == null) {
            finishWithMessage("Could not prepare the level export.");
            return;
        }

        Intent createDocument = new Intent(Intent.ACTION_CREATE_DOCUMENT);
        createDocument.addCategory(Intent.CATEGORY_OPENABLE);
        createDocument.setType("application/json");
        createDocument.putExtra(Intent.EXTRA_TITLE, fileName);

        try {
            startActivityForResult(createDocument, REQUEST_CREATE_DOCUMENT);
        } catch (ActivityNotFoundException exception) {
            Log.e(TAG, "No Android document picker is available.", exception);
            finishWithMessage("No Android document picker is available.");
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode != REQUEST_CREATE_DOCUMENT) {
            return;
        }

        if (resultCode != RESULT_OK || data == null || data.getData() == null) {
            finishWithMessage("Level export cancelled.");
            return;
        }

        Uri destination = data.getData();
        String json = getIntent().getStringExtra(EXTRA_JSON);
        try (OutputStream output = getContentResolver().openOutputStream(destination, "wt")) {
            if (output == null) {
                throw new IOException("The selected location could not be opened for writing.");
            }
            output.write(json.getBytes(StandardCharsets.UTF_8));
            finishWithMessage("Level exported successfully.");
        } catch (IOException exception) {
            Log.e(TAG, "Could not write the level JSON to the selected location.", exception);
            finishWithMessage("Level export failed: " + exception.getMessage());
        }
    }

    private void finishWithMessage(String message) {
        Toast.makeText(this, message, Toast.LENGTH_LONG).show();
        finish();
    }
}

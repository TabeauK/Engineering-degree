package com.app.UsosFix;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.TextView;

import androidx.appcompat.app.ActionBar;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.Volley;

import org.json.JSONException;
import org.json.JSONObject;

import static android.content.Intent.ACTION_VIEW;

public class InputPinActivity extends BaseActivity {
    private String Token;
    private EditText PIN;
    private Activity context;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_input_pin);

        ActionBar actionBar = getSupportActionBar();
        actionBar.hide();

        Token = "";
        context = this;
        Login();
    }
    private void Login() {
        String url = getResources().getString(R.string.base_url) + "/UsosAuthorization/OauthToken?env=Mobile";
        RequestQueue queue = Volley.newRequestQueue(this);

        JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        try {
                            Token = response.getString("token");
                            SendToken();
                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                final View activityView = findViewById(R.id.inputPinRelativeLayout);

                LayoutInflater layoutInflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
                View popUpLayout = layoutInflater.inflate(R.layout.pop_up_error, null);
                TextView popUpTextView = popUpLayout.findViewById(R.id.errorInfo);

                popUpTextView.setText(getString(R.string.authorization_error));
                PopupWindow popUp = new PopupWindow(context);
                popUp.setContentView(popUpLayout);
                popUp.setWidth(LinearLayout.LayoutParams.WRAP_CONTENT);
                popUp.setHeight(LinearLayout.LayoutParams.WRAP_CONTENT);
                popUp.setFocusable(false);
                popUp.setOutsideTouchable(false);
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) { ;
                    activityView.setForeground(new ColorDrawable(0xCC000000)); //ResourcesCompat.getDrawable(getResources(), R.drawable.button_background, null));
                }

                popUp.setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
                popUp.showAtLocation(activityView, Gravity.CENTER, 0, 0);

                Button OKbutton = popUpLayout.findViewById(R.id.OKbutton);
                OKbutton.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        popUp.dismiss();
                        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                            activityView.setForeground(new ColorDrawable(0x00));
                        }
                        Intent mainActivityIntent = new Intent(getApplicationContext(), MainActivity.class);
                        startActivity(mainActivityIntent);
                    }
                });
            }
        });

        queue.add(jsonObjectRequest);
    }

    private void SendToken() {
        String url = "https://apps.usos.pw.edu.pl/apps/authorize?oauth_token=" + Token + "&interactivity=minimal";
        Uri uri = Uri.parse(url);
        Intent intent = new Intent(ACTION_VIEW, uri);
        startActivity(intent);
        SetTextWatcher();
    }

    private void SetTextWatcher() {
        PIN = findViewById(R.id.PIN);
        final Button PinConfirmation_Button = findViewById(R.id.PINConfirmation_Button);
        final TextWatcher TextEditorWatcher = new TextWatcher() {
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            public void onTextChanged(CharSequence s, int start, int before, int count) {
                if (s.length() == 8)
                    PinConfirmation_Button.setEnabled(true);
                else
                    PinConfirmation_Button.setEnabled(false);
            }

            public void afterTextChanged(Editable s) { }
        };
        PIN.addTextChangedListener(TextEditorWatcher);
    }

    public void ConfirmPIN(View view) {
        String url = getResources().getString(R.string.base_url) + "/UsosAuthorization/AccessToken?pin=" + PIN.getText() + "&token=" + Token;
        // Instantiate the RequestQueue.
        RequestQueue queue = Volley.newRequestQueue(this);
        // Request a string response from the provided URL.
        JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        try {
                            AuthorizedToken = response.getString("token"); //ZAPAMIĘTAĆ

                            SharedPreferences sharedPref = getPreferences(Context.MODE_PRIVATE);
                            SharedPreferences.Editor editor = sharedPref.edit();
                            editor.putString(getString(R.string.AuthorizedToken), response.getString("token"));
                            editor.apply();

                            if (AuthorizedToken == null) {
                                ShowPopup(context, context.findViewById(R.id.inputPinRelativeLayout), getString(R.string.token_error), true);
                            }
                            else {
                                Intent ScheduleIntent = new Intent(getApplicationContext(), ScheduleActivity.class);
                                ScheduleIntent.putExtra("AuthorizedToken", AuthorizedToken);
                                startActivity(ScheduleIntent);
                            }
                        } catch (JSONException e) {
                            ShowPopup(context, findViewById(R.id.inputPinRelativeLayout), getString(R.string.authorization_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.inputPinRelativeLayout), getString(R.string.authorization_error), true);
            }
        });

        queue.add(jsonObjectRequest);
    }
}
package com.app.UsosFix;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MenuInflater;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;

import org.json.JSONException;
import org.json.JSONObject;

import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class BaseActivity extends AppCompatActivity {
    // Klasa służąca do wyświetlania menu
    // Klasa bazowa dla wszystkich widoków dostępnych po zalogowaniu

    protected String AuthorizedToken;
    protected JSONObject UserInfo; //informacje o użytkowniku pobrane z ep /Account/WhoAmI
    protected String language;
    private Activity context;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        context = this;

        if (savedInstanceState == null) {
            Bundle extras = getIntent().getExtras();
            if(extras == null) {
                AuthorizedToken = null;
            } else {
                AuthorizedToken = extras.getString("AuthorizedToken");
            }
        } else {
            AuthorizedToken = (String) savedInstanceState.getSerializable("AuthorizedToken");
        }
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.main_menu, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.menu_user_panel:
                Intent UserPanelIntent = new Intent(this, UserPanelActivity.class);
                UserPanelIntent.putExtra("AuthorizedToken", AuthorizedToken);
                startActivity(UserPanelIntent);
                return true;
            case R.id.menu_schedule:
                Intent ScheduleIntent = new Intent(this, ScheduleActivity.class);
                ScheduleIntent.putExtra("AuthorizedToken", AuthorizedToken);
                ScheduleIntent.putExtra("scheduleBase", "USOS");
                startActivity(ScheduleIntent);
                return true;
            case R.id.menu_message_box:
                Intent ConversationsIntent = new Intent(this, ConversationsActivity.class);
                ConversationsIntent.putExtra("AuthorizedToken", AuthorizedToken);
                startActivity(ConversationsIntent);
                return true;
            case R.id.menu_my_teams:
                Intent TeamsIntent = new Intent(this, TeamsActivity.class);
                TeamsIntent.putExtra("AuthorizedToken", AuthorizedToken);
                startActivity(TeamsIntent);
                return true;
            case R.id.menu_my_exchanges:
                Intent ExchangesIntent = new Intent(this, ExchangeActivity.class);
                ExchangesIntent.putExtra("AuthorizedToken", AuthorizedToken);
                startActivity(ExchangesIntent);
                return true;
            case R.id.menu_log_out:
                LogOut();
                Intent MainIntent = new Intent(this, MainActivity.class);
                startActivity(MainIntent);
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }
    // pobiera informacje o użytkowniku (imię, nazwisko, indeks, język, tryb ciemny) i zapisuje w polu UserInfo
    protected void GetUserInformation(final VolleyCallback callback) {
        if (AuthorizedToken == null) return;

        String url = getResources().getString(R.string.base_url) + "/Account/WhoAmI?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        UserInfo = response;
                        GetUserInformation_onResponseFun();
                        try {
                            callback.onSuccess();
                        } catch (JSONException e) {
                            ShowPopup(context, context.getWindow().getDecorView(), getString(R.string.user_info_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.getWindow().getDecorView(), getString(R.string.user_info_error), true);
            }
        });

        queue.add(jsonObjectRequest);
    }
    protected void GetUserInformation_onResponseFun()
    {
        SharedPreferences sharedPreferences = getSharedPreferences("userId", Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        try {
            editor.putString("userId", UserInfo.getString("id"));
        } catch (JSONException e) {
            e.printStackTrace();
        }
        editor.apply();
    }
    protected void ShowPopup(final Activity context, View activityView, String message, boolean isError)
    {
        LayoutInflater layoutInflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View popUpLayout = layoutInflater.inflate(R.layout.pop_up_success, null);
        TextView popUpTextView = popUpLayout.findViewById(R.id.successInfo);

        if (isError) {
            popUpLayout = layoutInflater.inflate(R.layout.pop_up_error, null);
            popUpTextView = popUpLayout.findViewById(R.id.errorInfo);
        }

        popUpTextView.setText(message);
        PopupWindow popUp = new PopupWindow(context);
        popUp.setContentView(popUpLayout);
        popUp.setWidth(LinearLayout.LayoutParams.WRAP_CONTENT);
        popUp.setHeight(LinearLayout.LayoutParams.WRAP_CONTENT);
        popUp.setFocusable(false);
        popUp.setOutsideTouchable(false);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) { ;
            activityView.setForeground(new ColorDrawable(0xCC000000));
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
            }
        });
    }
    //ustawia język aplikacji bazując na danych z UserInfo
    protected void SetLanguage() throws JSONException {
        Locale locale = new Locale("en");
        language = UserInfo.getString("preferredLanguage");
        if (language.contentEquals("Polish")) locale = new Locale("pl");

        Locale.setDefault(locale);
        Resources resources = this.getResources();
        Configuration config = resources.getConfiguration();
        config.setLocale(locale);
        resources.updateConfiguration(config, resources.getDisplayMetrics());
    }
    //pomaga skalować widok
    protected float convertDpToPx(Context context, float dp) {
        return dp * context.getResources().getDisplayMetrics().density;
    }

    private void LogOut() {
        String url = getResources().getString(R.string.base_url) + "/UsosAuthorization/LogOff?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.DELETE, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        Log.d("Response ", response);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.getWindow().getDecorView(), getString(R.string.log_out_error), true);
            }
        });

        queue.add(stringRequest);
    }
    public String getDayStringOld(String stringDate, Locale locale) throws ParseException {
        SimpleDateFormat formatter1 = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");
        Date date = formatter1.parse(stringDate);
        DateFormat formatter2 = new SimpleDateFormat("EEEE", locale);
        return formatter2.format(date);
    }
    protected void GetTeams() {
        String url = getResources().getString(R.string.base_url) + "/Teams/MyTeams?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        try {
                            GetTeams_onResponseFun(response);
                        } catch (JSONException | ParseException e) {
                            ShowPopup(context, context.getWindow().getDecorView(), getString(R.string.teams_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.getWindow().getDecorView(), getString(R.string.teams_error), true);
            }
        });

        queue.add(jsonObjectRequest);
    }
    protected void GetTeams_onResponseFun(JSONObject response) throws JSONException, ParseException { }
}

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
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.RadioButton;
import android.widget.RelativeLayout;
import android.widget.TableLayout;
import android.widget.TableRow;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatDelegate;
import androidx.core.content.res.ResourcesCompat;

import com.android.volley.AuthFailureError;
import com.android.volley.NetworkResponse;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.HttpHeaderParser;
import com.android.volley.toolbox.JsonArrayRequest;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.google.android.material.checkbox.MaterialCheckBox;
import com.google.android.material.switchmaterial.SwitchMaterial;
import com.google.gson.Gson;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.nio.ByteBuffer;
import java.nio.IntBuffer;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Dictionary;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.Locale;
import java.util.Map;

public class UserPanelActivity extends BaseActivity {

    private String role;
    private boolean visible;
    private Activity context;
    private ArrayList<Integer> chosenSubjectIds;
    private ArrayList<CheckBox> subjectCheckboxes;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        context = this;
        chosenSubjectIds = new ArrayList<>();
        subjectCheckboxes = new ArrayList<>();

        GetUserInformation(new VolleyCallback() {
            @Override
            public void onSuccess() throws JSONException { }
        });
        setContentView(R.layout.activity_user_panel);
    }
    @Override
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

        try {
            DisplayUserInformation();
        } catch (JSONException e) {
            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.user_info_display_error), true);
        }

        ShowPanels();
    }
    private void DisplayUserInformation() throws JSONException {
        visible = Boolean.parseBoolean(UserInfo.getString("visible"));
        SetRODOSwitch();
        language = UserInfo.getString("preferredLanguage");
        SetLanguage();

        LinearLayout userPanelScrollView = findViewById(R.id.userPanelScrollView);
        LayoutInflater inflater = (LayoutInflater)getApplicationContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        LinearLayout userInfoLinearLayout = (LinearLayout)inflater.inflate(R.layout.subject_class_type_schedule_card, userPanelScrollView, false);
        TextView userInfoTitle = userInfoLinearLayout.findViewById(R.id.classTypeTextView);
        userInfoTitle.setText(R.string.user_info);

        Button changeUsernameButton = findViewById(R.id.changeUsernameButton);
        changeUsernameButton.setOnClickListener(this::ChangeUsername);

        SwitchMaterial userRodoAgreementSwitch = findViewById(R.id.userRodoAgreementSwitch);
        userRodoAgreementSwitch.setOnCheckedChangeListener((this::ChangeRODOSettings));

        TextView nameTextView= findViewById(R.id.userNameTextView);
        TextView surnameTextView = findViewById(R.id.userSurnameTextView);
        TextView indexTextView = findViewById(R.id.indexNumberTextView);
        TextView usernameTextView = findViewById(R.id.usernameInfoTextView);
        TextView idTextView = findViewById(R.id.userIdTextView);

        nameTextView.setText(UserInfo.getString("name"));
        surnameTextView.setText(UserInfo.getString("surname"));
        indexTextView.setText(UserInfo.getString("studentNumber"));
        usernameTextView.setText(UserInfo.getString("username"));
        String id = UserInfo.getString("id");
        idTextView.setText("#".concat("000".substring(id.length(), 3)).concat(id));

        role = UserInfo.getString("role");
    }

    private void SetRODOSwitch() {
        SwitchMaterial RODOSwitch = findViewById(R.id.userRodoAgreementSwitch);
        RODOSwitch.setChecked(visible);
    }
    @Override
    protected void SetLanguage() {
        RadioButton englishRadioButton = findViewById(R.id.englishRadioButton);
        RadioButton polishRadioButton = findViewById(R.id.polishRadioButton);
        Locale locale = new Locale("en");

        if (language.contentEquals("English")) {
            englishRadioButton.setChecked(true);
        }
        else if (language.contentEquals("Polish")) {
            polishRadioButton.setChecked(true);
            locale = new Locale("pl");
        }

        Locale.setDefault(locale);
        Resources resources = this.getResources();
        Configuration config = resources.getConfiguration();
        config.setLocale(locale);
        resources.updateConfiguration(config, resources.getDisplayMetrics());
    }

    private void ShowPanels() {
        LinearLayout userPanelScrollView = findViewById(R.id.userPanelScrollView);
        RelativeLayout leaderPanel = (RelativeLayout) ((LayoutInflater)getSystemService(Context.LAYOUT_INFLATER_SERVICE)).inflate(R.layout.leader_panel, null);
        RelativeLayout.LayoutParams params = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
        params.setMargins(0, 0, 0, 10);
        leaderPanel.setLayoutParams(params);

        if (role.equals("Admin")) {
            GetSubjectIds(leaderPanel);
            userPanelScrollView.addView(leaderPanel);
            LinearLayout adminPanel = (LinearLayout) ((LayoutInflater)getSystemService(Context.LAYOUT_INFLATER_SERVICE)).inflate(R.layout.admin_panel, null);
            userPanelScrollView.addView(adminPanel);
            DisplayUserInRoles();
            return;
        }
        if (role.equals("Leader")) { //starosta
            GetSubjectIds(leaderPanel);
            userPanelScrollView.addView(leaderPanel);
            return;
        }
    }
    private void GetSubjectIds(RelativeLayout leaderPanel) {
        if (AuthorizedToken == null)
            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.token_error), true);

        String url = getResources().getString(R.string.base_url) + "/Timetable/UserGroups?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        try {
                            GetExchangesSummaryBySubject(leaderPanel, response);
                        } catch (JSONException e) {
                            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.exchanges_info_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.exchanges_info_error), true);
            }
        });

        queue.add(jsonArrayRequest);
    }

    private void GetExchangesSummaryBySubject(RelativeLayout leaderPanel, JSONArray userGroups) throws JSONException {
        String url;
        RequestQueue queue = Volley.newRequestQueue(this);
        TableLayout subjectsTable = leaderPanel.findViewById(R.id.subjectsTable);
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);

        ArrayList<JSONObject> subjects = new ArrayList<JSONObject>();
        ArrayList<String> subjectNames = new ArrayList<String>();
        for (int i = 0; i < userGroups.length(); i++) {
            JSONObject userGroup = userGroups.getJSONObject(i);
            String subjectName = userGroup.getJSONObject("subject").getJSONObject("name").getString(getResources().getConfiguration().locale.getLanguage());
            if (!subjectNames.contains(subjectName)) {
                subjects.add(userGroup);
                subjectNames.add(subjectName);
            }
        }

        for (int i = 0; i < subjects.size(); i++) {
            final String subjectId = subjects.get(i).getJSONObject("subject").getString("id");
            final String subjectName = subjectNames.get(i);
            url = getResources().getString(R.string.base_url) + "/Exchanges/ExchangesSummaryBySubject?token=" + AuthorizedToken + "&subjectId=" + subjectId;

            final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, url, null,
                    new Response.Listener<JSONObject>() {
                        @Override
                        public void onResponse(JSONObject response) {
                            //dodać info do tabelki w panelu starosty
                            TableRow row = new TableRow(getApplicationContext());
                            row.setGravity(Gravity.CENTER);
                            params.setMargins(40, 10, 10, 10);
                            //row.setPadding(0, 10, 10, 10);
                            row.setLayoutParams(params);

                            CheckBox subjectCheckBox = new CheckBox(getApplicationContext());
                            subjectCheckBox.setText(subjectName);
                            subjectCheckBox.setSingleLine(false);
                            subjectCheckBox.setMaxWidth(80);
                            subjectCheckBox.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_medium));
                            subjectCheckBox.setTextColor(getResources().getColor(R.color.colorBlack));
                            subjectCheckBox.setTextSize(15);
                            subjectCheckBox.setPadding(10, 0, 10, 0);
                            subjectCheckBox.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
                                @Override
                                public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                                    if (isChecked)
                                        chosenSubjectIds.add(Integer.parseInt(subjectId));
                                    else
                                        chosenSubjectIds.remove(chosenSubjectIds.indexOf(Integer.parseInt(subjectId)));
                                }
                            });
                            subjectCheckboxes.add(subjectCheckBox);
                            row.addView(subjectCheckBox);

                            TextView submittedTextView = new TextView(getApplicationContext());
                            submittedTextView.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_medium));
                            submittedTextView.setTextColor(getResources().getColor(R.color.colorBlack));
                            submittedTextView.setTextSize(15);
                            submittedTextView.setGravity(Gravity.CENTER);

                            TextView acceptedTextView = new TextView(getApplicationContext());
                            acceptedTextView.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_medium));
                            acceptedTextView.setTextColor(getResources().getColor(R.color.colorBlack));
                            acceptedTextView.setTextSize(15);
                            acceptedTextView.setGravity(Gravity.CENTER);

                            TextView rejectedTextView = new TextView(getApplicationContext());
                            rejectedTextView.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_medium));
                            rejectedTextView.setTextColor(getResources().getColor(R.color.colorBlack));
                            rejectedTextView.setTextSize(15);
                            rejectedTextView.setGravity(Gravity.CENTER);

                            try {
                                submittedTextView.setText(response.getString("submitted"));
                                acceptedTextView.setText(response.getString("accepted"));
                                rejectedTextView.setText(response.getString("rejected"));
                            } catch (JSONException e) {
                                ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.exchanges_info_display_error), true);
                            }
                            row.addView(submittedTextView);
                            row.addView(acceptedTextView);
                            row.addView(rejectedTextView);

                            subjectsTable.addView(row);
                        }
                    }, new Response.ErrorListener() {
                @Override
                public void onErrorResponse(VolleyError error) {
                    ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.exchanges_info_error), true);
                }
            });

            queue.add(jsonObjectRequest);
        }
    }
    public void SendToDeansOffice(View view) {
        if (chosenSubjectIds.size() == 0) {
            ShowPopup(context, findViewById(R.id.userPanelLinearLayout), getString(R.string.no_chosen_subjects_error), true);
            return;
        }

        String url = getResources().getString(R.string.base_url) + "/Admin/SendBatchEmail?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);
        ArrayList<Integer> idsWithError = new ArrayList<>();

        JSONArray subjectIdsArray = new JSONArray();
        for (Integer id:chosenSubjectIds) {
            subjectIdsArray.put(id);
        }

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.POST, url, subjectIdsArray,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        ShowPopup(context, findViewById(R.id.userPanelLinearLayout), getString(R.string.send_email_success), false);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.userPanelLinearLayout), getString(R.string.send_email_error), true);
            }
        }) {
            @Override
            protected Response<JSONArray> parseNetworkResponse(NetworkResponse response) {
                return Response.success(null, HttpHeaderParser.parseCacheHeaders(response));
            }
        };

        queue.add(jsonArrayRequest);
    }
    private void DisplayUserInRoles() {
        String url = getResources().getString(R.string.base_url) + "/Roles/AdminsAndLeaders?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        try {
                            JSONArray leadersList = new JSONArray();
                            JSONArray adminsList = new JSONArray();
                            for (int i = 0; i < response.length(); i++) {
                                if (response.getJSONObject(i).getString("role").equals("Leader"))
                                    leadersList.put(response.getJSONObject(i));
                                else if (response.getJSONObject(i).getString("role").equals("Admin"))
                                    adminsList.put(response.getJSONObject(i));
                            }
                            DisplayLeaders(leadersList);
                            DisplayAdmins(adminsList);
                        } catch (JSONException e) {
                            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.admins_leaders_display_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.admins_leaders_error), true);
            }
        });

        queue.add(jsonArrayRequest);
    }
    private void DisplayLeaders(JSONArray leaders) throws JSONException {
        if (leaders.length() == 0) return;

        TableLayout leadersTable = findViewById(R.id.leaderListTable);

        for (int i = 0; i < leaders.length(); i++) {
            TableRow row = new TableRow(this);
            row.setPadding(0, 5, 0, 5);
            JSONObject leader = leaders.getJSONObject(i);

            final String studentNumber = leader.getString("studentNumber");
            TextView indexTextView = new TextView(this);
            indexTextView.setText(leader.getString("name").concat(" ").concat(leader.getString("surname")));
            indexTextView.setTextSize(15);
            indexTextView.setTextColor(this.getResources().getColor(R.color.colorBlack));
            indexTextView.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_medium));

            Button button = new Button(this);
            button.setBackgroundResource(R.drawable.red_button_background);
            button.setText(R.string.remove);
            button.setTextColor(this.getResources().getColor(R.color.colorWhite));
            button.setTextSize(15);
            button.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_medium));
            button.setAllCaps(false);
            button.setMinHeight(0);
            button.setMinimumHeight(0);
            button.setHeight((int) convertDpToPx(this, 40f));
            button.setPadding(0, 0, 0, 0);
            button.setOnClickListener(new View.OnClickListener() {

                @Override
                public void onClick(View v) {
                    //remove leader
                    RemoveRoleClass removeRoleClass = new RemoveRoleClass();
                    ShowPermissionPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.remove_leader_question, studentNumber), studentNumber, removeRoleClass);
                }
            });
            //row.addView(checkbox);
            row.addView(indexTextView);
            row.addView(button);
            leadersTable.addView(row);
        }


    }
    private void DisplayAdmins(JSONArray admins) throws JSONException {
        if (admins.length() == 0) return;

        TableLayout adminsTable = findViewById(R.id.adminListTable);

        for (int i = 0; i < admins.length(); i++) {
            TableRow row = new TableRow(this);
            row.setPadding(0, 5, 0, 5);
            JSONObject admin = admins.getJSONObject(i);

            final String studentNumber = admin.getString("studentNumber");
            TextView indexTextView = new TextView(this);
            indexTextView.setText(admin.getString("name").concat(" ").concat(admin.getString("surname")));
            indexTextView.setTextSize(15);
            indexTextView.setTextColor(this.getResources().getColor(R.color.colorBlack));
            indexTextView.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_medium));

            Button button = new Button(this);
            button.setBackgroundResource(R.drawable.red_button_background);
            button.setText(R.string.remove);
            button.setTextColor(this.getResources().getColor(R.color.colorWhite));
            button.setTextSize(15);
            button.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_medium));
            button.setAllCaps(false);
            button.setMinHeight(0);
            button.setMinimumHeight(0);
            button.setHeight((int) convertDpToPx(this, 40f));
            button.setPadding(0, 0, 0, 0);
            button.setOnClickListener(new View.OnClickListener() {

                @Override
                public void onClick(View v) {
                    //remove leader
                    RemoveRoleClass removeRoleClass = new RemoveRoleClass();
                    ShowPermissionPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.remove_admin_question, studentNumber), studentNumber, removeRoleClass);
                }
            });

            row.addView(indexTextView);
            row.addView(button);
            adminsTable.addView(row);
        }
    }
    private void RemoveUserRole(String index) {
        String url = getResources().getString(R.string.base_url) + "/Roles/SetRoleByStudentNumber?token=" + AuthorizedToken + "&studentNumber=" + index + "&roleString=User";
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.remove_role_success), false);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.remove_role_error), true);
            }
        });

        queue.add(stringRequest);
    }
    public void AddLeader(View view) {
        EditText indexEditText = findViewById(R.id.addLeaderEditText);
        final String index = indexEditText.getText().toString();
        if (ValidateIndex(index)) {
            AddLeaderClass addLeaderClass = new AddLeaderClass();
            ShowPermissionPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.add_leader_question, index), index, addLeaderClass);
        }
    }
    public void AddAdmin(View view) {
        EditText indexEditText = findViewById(R.id.addAdminEditText);
        String index = indexEditText.getText().toString();
        if (ValidateIndex(index)) {
            AddAdminClass addAdminClass = new AddAdminClass();
            ShowPermissionPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.add_admin_question, index), index, addAdminClass);
        }
    }
    private boolean ValidateIndex(String index) {
        if (index.length() == 6) {
            if (Integer.parseInt(index) > 0 && Integer.parseInt(index) <= 999999)
                return true;
            else
                ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.index_error), true);
        }
        else
            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.index_error), true);
        return false;
    }

    public void ChangeUsername(View view) {
        EditText usernameEditText = findViewById(R.id.usernameEditText);
        String newUsername = usernameEditText.getText().toString();
        if (newUsername.length() < 1) {
            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.too_short_username), true);
            return;
        }

        String url = getResources().getString(R.string.base_url) + "/Account/SetUsername?token=" + AuthorizedToken + "&username=" + newUsername;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.change_username_success), false);
                        TextView usernameTextView = findViewById(R.id.usernameInfoTextView);
                        usernameTextView.setText(newUsername);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.change_username_error), true);
            }
        });

        queue.add(stringRequest);
    }

    public void ChangeRODOSettings(View view, boolean isVisible) {
        SwitchMaterial RODOSwitch = (SwitchMaterial)view; //findViewById(R.id.userRodoAgreementSwitch);
        String url = getResources().getString(R.string.base_url) + "/Account/SetVisibility?token=" + AuthorizedToken + "&visible=" + isVisible;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        if (isVisible)
                            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.reveal_yourself_success), false);
                        else
                            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.conceal_yourself_success), false);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                if (isVisible)
                    ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.reveal_yourself_error), true);
                else
                    ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.conceal_yourself_error), true);
            }
        });

        queue.add(stringRequest);
    }

    public void ChooseEnglish(View view) {
        ChangeLanguage("English", new VolleyCallback() {
            @Override
            public void onSuccess() throws JSONException {
                Locale locale = new Locale("en");
                Locale.setDefault(locale);
                Resources resources = getResources();
                Configuration config = resources.getConfiguration();
                config.setLocale(locale);
                resources.updateConfiguration(config, resources.getDisplayMetrics());

                Intent UserPanelIntent = new Intent(getApplicationContext(), UserPanelActivity.class);
                UserPanelIntent.putExtra("AuthorizedToken", AuthorizedToken);
                startActivity(UserPanelIntent);
            }
        });
    }

    public void ChoosePolish(View view) {
        ChangeLanguage("Polish", new VolleyCallback() {
            @Override
            public void onSuccess() throws JSONException {
                Locale locale = new Locale("pl");
                Locale.setDefault(locale);
                Resources resources = getResources();
                Configuration config = resources.getConfiguration();
                config.setLocale(locale);
                resources.updateConfiguration(config, resources.getDisplayMetrics());

                Intent UserPanelIntent = new Intent(getApplicationContext(), UserPanelActivity.class);
                UserPanelIntent.putExtra("AuthorizedToken", AuthorizedToken);
                startActivity(UserPanelIntent);
            }
        });
    }
    public void ChangeLanguage(String lang, final VolleyCallback callback) {
        String url = getResources().getString(R.string.base_url) + "/Account/SetLanguage?token=" + AuthorizedToken + "&languageString=" + lang;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        try {
                            callback.onSuccess();
                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.change_language_error), true);
            }
        });

        queue.add(stringRequest);
    }
    protected void ShowPermissionPopup(final Activity context, View activityView, String message, String studentId, ManagePermissions permissions)
    {
        LayoutInflater layoutInflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View popUpLayout = layoutInflater.inflate(R.layout.pop_up_permissions, null);
        TextView popUpTextView = popUpLayout.findViewById(R.id.permissionQuestion);

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

        Button noButton = popUpLayout.findViewById(R.id.noButton);
        noButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                popUp.dismiss();
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                    activityView.setForeground(new ColorDrawable(0x00));
                }
            }
        });
        Button yesButton = popUpLayout.findViewById(R.id.yesButton);
        yesButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                permissions.CallAPI(studentId);
                popUp.dismiss();
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                    activityView.setForeground(new ColorDrawable(0x00));
                }
            }
        });
    }

    public void StartNewSemester(View view) {
        final View activityView = context.findViewById(R.id.userPanelLinearLayout);
        LayoutInflater layoutInflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View popUpLayout = layoutInflater.inflate(R.layout.pop_up_permissions, null);
        TextView popUpTextView = popUpLayout.findViewById(R.id.permissionQuestion);

        popUpTextView.setText(R.string.new_semester_question);
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

        Button noButton = popUpLayout.findViewById(R.id.noButton);
        noButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                popUp.dismiss();
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                    activityView.setForeground(new ColorDrawable(0x00));
                }
            }
        });
        Button yesButton = popUpLayout.findViewById(R.id.yesButton);
        yesButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                CallNewSemesterRequest();
                popUp.dismiss();
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                    activityView.setForeground(new ColorDrawable(0x00));
                }
            }
        });
    }

    private void CallNewSemesterRequest() {
        String url = getResources().getString(R.string.base_url) + "/Admin/ChangeToNextTerm?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.POST, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.change_term_success), false);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.change_term_error), true);
            }
        });

        queue.add(stringRequest);
    }

    public void CalculateExchanges(View view) {
        if (chosenSubjectIds.size() == 0) {
            ShowPopup(context, findViewById(R.id.userPanelLinearLayout), getString(R.string.no_chosen_subjects_error), true);
            return;
        }

        String url = getResources().getString(R.string.base_url) + "/Exchanges/RealizeExchangesInSubjects?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        JSONArray subjectIdsArray = new JSONArray();
        for (Integer id:chosenSubjectIds) {
            subjectIdsArray.put(id);
        }

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.PUT, url, subjectIdsArray,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        ShowPopup(context, findViewById(R.id.userPanelLinearLayout), getString(R.string.calculate_exchanges_success), false);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.userPanelLinearLayout), getString(R.string.calculate_exchanges_error), true);
            }
        }) {
            @Override
            protected Response<JSONArray> parseNetworkResponse(NetworkResponse response) {
                return Response.success(null, HttpHeaderParser.parseCacheHeaders(response));
            }
        };

        queue.add(jsonArrayRequest);
    }

    public void SubjectsCheckboxOnClickListener(View view) {
        CheckBox subjectCheckbox = (CheckBox)view;
        if (subjectCheckbox.isChecked()) {
            //aktualizacja widoku
            for(int i = 0; i < subjectCheckboxes.size(); i++)
                subjectCheckboxes.get(i).setChecked(true);
        }
        else {
            //aktualizacja widoku
            for(int i = 0; i < subjectCheckboxes.size(); i++)
                subjectCheckboxes.get(i).setChecked(false);
        }
    }

    private interface ManagePermissions {
        void CallAPI(String studentId);
    }
    private class AddLeaderClass implements ManagePermissions {
        public void CallAPI(String studentId) {
            String url = getResources().getString(R.string.base_url) + "/Roles/SetRoleByStudentNumber?token=" + AuthorizedToken + "&studentNumber=" + studentId + "&roleString=Leader";
            RequestQueue queue = Volley.newRequestQueue(getApplicationContext());

            final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                    new Response.Listener<String>() {
                        @Override
                        public void onResponse(String response) {
                            LinearLayout userPanelScrollView = findViewById(R.id.userPanelScrollView);
                            if (userPanelScrollView.getChildCount() == 4)
                                userPanelScrollView.removeViewAt(3); //removing admin panel
                            userPanelScrollView.removeViewAt(2); //removing leader panel
                            ShowPanels();

                            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.add_leader_success), false);
                        }
                    }, new Response.ErrorListener() {
                @Override
                public void onErrorResponse(VolleyError error) {
                    ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.add_leader_error), true);
                }
            }) {
                @Override
                protected Map<String, String> getParams()
                {
                    Map<String, String>  params = new HashMap<String, String>();
                    params.put("token", AuthorizedToken);
                    params.put("studentNumber", studentId);
                    params.put("roleString", "Leader");

                    return params;
                }
            };

            queue.add(stringRequest);
        }
    }
    private class AddAdminClass implements ManagePermissions {
        public void CallAPI(String studentId) {
            String url = getResources().getString(R.string.base_url) + "/Roles/SetRoleByStudentNumber?token=" + AuthorizedToken + "&studentNumber=" + studentId + "&roleString=Admin";
            RequestQueue queue = Volley.newRequestQueue(getApplicationContext());

            final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                    new Response.Listener<String>() {
                        @Override
                        public void onResponse(String response) {
                            LinearLayout userPanelScrollView = findViewById(R.id.userPanelScrollView);
                            if (userPanelScrollView.getChildCount() == 4)
                                userPanelScrollView.removeViewAt(3); //removing admin panel
                            userPanelScrollView.removeViewAt(2); //removing leader panel
                            ShowPanels();

                            ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.add_admin_success), false);
                        }
                    }, new Response.ErrorListener() {
                @Override
                public void onErrorResponse(VolleyError error) {
                    ShowPopup(context, context.findViewById(R.id.userPanelLinearLayout), getString(R.string.add_admin_error), true);
                }
            });

            queue.add(stringRequest);
        }
    }
    private class RemoveRoleClass implements ManagePermissions {
        public void CallAPI(String studentId) {
            RemoveUserRole(studentId);

            LinearLayout userPanelScrollView = findViewById(R.id.userPanelScrollView);
            if (userPanelScrollView.getChildCount() == 4)
                userPanelScrollView.removeViewAt(3); //removing admin panel
            userPanelScrollView.removeViewAt(2); //removing leader panel

            try {
                if (studentId.equals(UserInfo.getString("studentNumber")))
                    role = "User";
            } catch (JSONException e) {
                e.printStackTrace();
            }
            ShowPanels();
        }
    }
}
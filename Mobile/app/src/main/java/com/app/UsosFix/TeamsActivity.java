package com.app.UsosFix;

import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Pair;
import android.view.View;
import android.widget.AdapterView;
import android.widget.AutoCompleteTextView;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TableLayout;
import android.widget.TableRow;
import android.widget.TextView;

import androidx.core.content.res.ResourcesCompat;

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

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.text.ParseException;
import java.util.ArrayList;

public class TeamsActivity extends BaseActivity {

    private class TeamData {
        String subjectName;
        String subjectId;
        String id;
        String selectedUserId;
        ArrayList<Pair<String, String>> members; //displayName, String id
        ArrayList<Pair<String, String>> invitations;
    }
    private ArrayList<TeamData> teams;
    private ArrayList<TeamData> invitations;
    private Activity context;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_teams);
        context = this;

        teams = new ArrayList<TeamData>();
        invitations = new ArrayList<TeamData>();
        GetTeams();
    }
    private void AcceptTeamInvitation(String invitationId) {
        String url = getResources().getString(R.string.base_url) + "/Teams/AcceptInvitation?token=" + AuthorizedToken + "&invitationId=" + invitationId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.teamsLinearLayout), getString(R.string.accept_team_success), false);
                        GetTeams();
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.teamsLinearLayout), getString(R.string.accept_team_error), true);
            }
        });

        queue.add(stringRequest);
    }
    private void RejectTeamInvitation(String invitationId) {
        String url = getResources().getString(R.string.base_url) + "/Teams/DeleteInvitation?token=" + AuthorizedToken + "&invitationId=" + invitationId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.teamsLinearLayout), getString(R.string.reject_team_success), false);
                        GetTeams();
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.teamsLinearLayout), getString(R.string.reject_team_error), true);
            }
        });

        queue.add(stringRequest);
    }
    @Override
    protected void GetTeams_onResponseFun(JSONObject response) throws JSONException, ParseException {
        teams = new ArrayList<TeamData>();
        String currentLang = getResources().getConfiguration().locale.getLanguage();
        
        JSONArray teamsArray = response.getJSONArray("partOf");
        for (int i = 0; i < teamsArray.length(); i++) {
            final JSONObject teamObject = teamsArray.getJSONObject(i);
            JSONArray usersInTeam = teamObject.getJSONArray("users");
            JSONArray invitedToTeam = teamObject.getJSONArray("invitations");
            if (usersInTeam.length() + invitedToTeam.length() > 1) {
                TeamData teamData = new TeamData();

                teamData.id = teamObject.getString("id");
                teamData.subjectName = teamObject.getJSONObject("subject").getJSONObject("name").getString(currentLang);
                teamData.subjectId = teamObject.getJSONObject("subject").getString("id");

                teamData.members = new ArrayList<>();
                for (int j = 0; j < usersInTeam.length(); j++) {
                    final JSONObject user = usersInTeam.getJSONObject(j);
                    teamData.members.add(new Pair<String, String>(user.getString("displayName"), user.getString("id")));
                }

                teamData.invitations = new ArrayList<>();
                for (int j = 0; j < invitedToTeam.length(); j++) {
                    final JSONObject invitation = invitedToTeam.getJSONObject(j);
                    teamData.invitations.add(new Pair<String, String>(invitation.getJSONObject("invited").getString("displayName").concat(" (").concat(getString(R.string.pending)).concat(")"), invitation.getString("id")));
                }

                teams.add(teamData);
            }
        }

        DisplayTeams();

        invitations = new ArrayList<>();
        JSONArray invitationsArray = response.getJSONArray("invitedTo");
        for (int i = 0; i < invitationsArray.length(); i++) {
            final JSONObject teamObject = invitationsArray.getJSONObject(i);
            JSONArray usersInTeam = teamObject.getJSONArray("users");
            JSONArray invitedToTeam = teamObject.getJSONArray("invitations");

            if (usersInTeam.length() + invitedToTeam.length() > 1) {
                TeamData teamData = new TeamData();

                teamData.subjectName = teamObject.getJSONObject("subject").getJSONObject("name").getString(currentLang);
                teamData.id = teamObject.getString("id");

                teamData.members = new ArrayList<>();
                for (int j = 0; j < usersInTeam.length(); j++) {
                    final JSONObject user = usersInTeam.getJSONObject(j);
                    teamData.members.add(new Pair<String, String>(user.getString("displayName"), user.getString("id")));
                }

                teamData.invitations = new ArrayList<>();
                for (int j = 0; j < invitedToTeam.length(); j++) {
                    final JSONObject invitation = invitedToTeam.getJSONObject(j);
                    teamData.invitations.add(new Pair<String, String>(invitation.getJSONObject("invited").getString("displayName").concat(" (").concat(getString(R.string.pending)).concat(")"), invitation.getString("id")));
                }

                invitations.add(teamData);
            }
        }

        DisplayInvitations();
    }

    private void DisplayInvitations() {
        LinearLayout teamsLinearLayout = findViewById(R.id.teamScrollView);
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
        params.setMargins(0, 10, 0, 20);

        for (int i = 0; i < invitations.size(); i++) {
            TeamData teamData = invitations.get(i);
            final int teamId = Integer.parseInt(teamData.id);

            LinearLayout teamView = (LinearLayout) getLayoutInflater().inflate(R.layout.team_invitation_element, null);
            teamView.setLayoutParams(params);

            TextView subjectName = teamView.findViewById(R.id.team_subjectName);
            subjectName.setText(teamData.subjectName);

            params.setMargins(0, 5, 0, 5);
            TableLayout teamMembersTable = teamView.findViewById(R.id.teamMembersTable);

            int teamMembersNumber = teamData.members.size();
            int teamInvitationsNumber = teamData.invitations.size();

            if (teamInvitationsNumber > 0) {
                View tabelsDivider = teamView.findViewById(R.id.teamMembersAndInvitationsDivider);
                tabelsDivider.setVisibility(View.VISIBLE);
            }

            for (int j = 0; j < teamMembersNumber; j++) {
                Pair<String, String> member = teamData.members.get(j);
                TableRow row = new TableRow(this);
                row.setPadding(0, 5, 0, 5);

                TextView student = new TextView(this);
                student.setPadding(50, 15, 0, 15);
                student.setText(member.first);
                student.setTextSize(15);
                student.setTextColor(this.getResources().getColor(R.color.colorBlack));
                student.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_bold));
                row.addView(student);
                teamMembersTable.addView(row);
            }

            TableLayout teamInvitationsTable = teamView.findViewById(R.id.teamInvitationsTable);

            for (int j = 0; j < teamInvitationsNumber; j++) {
                Pair<String, String> invitation = teamData.invitations.get(j);
                TableRow row = new TableRow(this);
                row.setPadding(0, 5, 0, 5);

                TextView student = new TextView(this);
                student.setPadding(50, 15, 0, 15);
                student.setText(invitation.first);
                student.setTextSize(15);
                student.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_bold));
                row.addView(student);

                String invitationId = invitation.second;
                Button acceptButton = teamView.findViewById(R.id.acceptButton);
                Button rejectButton = teamView.findViewById(R.id.rejectButton);
                acceptButton.setOnClickListener(v -> AcceptTeamInvitation(invitationId));
                rejectButton.setOnClickListener(v -> RejectTeamInvitation(invitationId));

                teamInvitationsTable.addView(row);
            }

            teamsLinearLayout.addView(teamView);
        }
    }

    private void DisplayTeams() throws JSONException {
        LinearLayout teamsLinearLayout = findViewById(R.id.teamScrollView);
        teamsLinearLayout.removeAllViews();
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);

        for (int i = 0; i < teams.size(); i++) {
            TeamData teamData = teams.get(i);
            final int teamId = Integer.parseInt(teamData.id);

            params.setMargins(0, 10, 0, 20);
            LinearLayout teamView = (LinearLayout) getLayoutInflater().inflate(R.layout.team_element, null);
            teamView.setLayoutParams(params);

            TextView subjectName = teamView.findViewById(R.id.team_subjectName);
            subjectName.setText(teamData.subjectName);

            SharedPreferences sharedPreferences = getSharedPreferences("userId", Context.MODE_PRIVATE);
            String id = sharedPreferences.getString("userId", "-1");

            final int myId = Integer.parseInt(id);
            Button leaveButton = teamView.findViewById(R.id.leaveTeamButton);
            int finalI = i;
            leaveButton.setOnClickListener(v -> {
                if (myId >= 0) DeleteUserFromTeam(teamId, myId, finalI);
            });

            Button inviteButton = teamView.findViewById(R.id.inviteToTeamButton);
            inviteButton.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    InviteToTeam(teamData.selectedUserId, teamData.subjectId);
                }
            });

            SetTextWatcher(teamView, teamData);

            params.setMargins(0, 30, 0, 30);
            TableLayout teamMembersTable = teamView.findViewById(R.id.teamMembersTable);

            int teamMembersNumber = teamData.members.size();
            int teamInvitationsNumber = teamData.invitations.size();

            for (int j = 0; j < teamMembersNumber; j++) {
                Pair<String, String> member = teamData.members.get(j);
                TableRow row = new TableRow(this);
                row.setPadding(0, 10, 0, 10);

                TextView student = new TextView(this);
                student.setPadding(50, 0, 0, 0);
                student.setText(member.first);
                student.setTextSize(15);
                student.setTextColor(this.getResources().getColor(R.color.colorBlack));
                student.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_bold));
                row.addView(student);

                Button button = new Button(this);
                button.setBackgroundResource(R.drawable.red_button_background);
                button.setText(R.string.remove);
                button.setTextColor(this.getResources().getColor(R.color.colorWhite));
                button.setTextSize(17);
                button.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_bold));
                button.setAllCaps(false);
                button.setMinHeight(0);
                button.setMinimumHeight(0);
                button.setHeight((int) getResources().getDimension(R.dimen.button_height));
                button.setOnClickListener(v -> {
                    int userId = Integer.parseInt(member.second);
                    DeleteUserFromTeam(teamId, userId, finalI);
                });

                if (member.second.equals(id) || teamMembersNumber + teamInvitationsNumber <= 2) {
                    button.setEnabled(false);
                    button.setVisibility(View.INVISIBLE);
                }
                row.addView(button);

                teamMembersTable.addView(row);
            }

            TableLayout teamInvitationsTable = teamView.findViewById(R.id.teamInvitationsTable);

            if (teamInvitationsNumber > 0) {
                View divider = teamView.findViewById(R.id.teamMembersAndInvitationsDivider);
                divider.setVisibility(View.VISIBLE);
            }

            for (int j = 0; j < teamInvitationsNumber; j++) {
                Pair<String, String> invitation = teamData.invitations.get(j);
                TableRow row = new TableRow(this);
                row.setPadding(0, 10, 0, 10);

                TextView student = new TextView(this);
                student.setPadding(50, 0, 0, 0);
                student.setText(invitation.first);
                student.setTextSize(15);
                student.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_bold));
                row.addView(student);

                String invitationId = invitation.second;

                Button button = new Button(this);
                button.setTextColor(this.getResources().getColor(R.color.colorWhite));
                button.setBackgroundResource(R.drawable.red_button_background);
                button.setText(R.string.remove);
                button.setTextSize(17);
                button.setTypeface(ResourcesCompat.getFont(context, R.font.barlow_bold));
                button.setAllCaps(false);
                button.setMinHeight(0);
                button.setMinimumHeight(0);
                button.setHeight((int) getResources().getDimension(R.dimen.button_height));
                button.setOnClickListener(v -> StopInvitingToTeam(invitationId, finalI));

                if (teamMembersNumber + teamInvitationsNumber <= 2) {
                    button.setEnabled(false);
                    button.setVisibility(View.INVISIBLE);
                }
                row.addView(button);

                teamInvitationsTable.addView(row);
            }

            teamsLinearLayout.addView(teamView);
        }
    }

    private void InviteToTeam(String invitedId, String subjectId) {
        String url = getResources().getString(R.string.base_url) + "/Teams/InviteUser?token=" + AuthorizedToken + "&invitedId=" + invitedId + "&subjectId=" + subjectId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.PUT, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        //refresh the view
                        GetTeams();
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.teamsLinearLayout), getString(R.string.inv_to_team_error), true);
            }
        }) {
            @Override
            protected Response<JSONObject> parseNetworkResponse(NetworkResponse response) {
                return Response.success(null, HttpHeaderParser.parseCacheHeaders(response));
            }
        };

        queue.add(jsonObjectRequest);
    }

    private void StopInvitingToTeam(String invitationId, int teamIndex) {
        String url = getResources().getString(R.string.base_url) + "/Teams/DeleteInvitation?token=" + AuthorizedToken + "&invitationId=" + invitationId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.teamsLinearLayout), getString(R.string.remove_team_inv_success), false);
                        GetTeams();
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.teamsLinearLayout), getString(R.string.remove_team_inv_success), true);
            }
        });

        queue.add(stringRequest);
    }

    private void DeleteUserFromTeam(int teamId, int userId, int teamElementIndex) {
        String url = getResources().getString(R.string.base_url) + "/Teams/DeleteUserFromTeam?token=" + AuthorizedToken + "&teamId=" + teamId + "&userId=" + userId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        ShowPopup(context, context.findViewById(R.id.teamsLinearLayout), getString(R.string.remove_user_from_team_success), false);
                        GetTeams();
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, context.findViewById(R.id.teamsLinearLayout), getString(R.string.remove_user_from_team_error), true);
            }
        });

        queue.add(stringRequest);
    }
    private void SetTextWatcher(LinearLayout teamElement, TeamData teamData) {
        AutoCompleteTextView teamUserSearchAutoComplete = teamElement.findViewById(R.id.teamUserSearchAutoComplete);
        final TextWatcher TextEditorWatcher = new TextWatcher() {
            public void beforeTextChanged(CharSequence s, int start, int count, int after) { }

            public void onTextChanged(CharSequence s, int start, int before, int count) {
                ArrayList<String> usernames = FindMatchingUsers(s.toString(), teamUserSearchAutoComplete, teamData);
            }

            public void afterTextChanged(Editable s) { }
        };
        teamUserSearchAutoComplete.addTextChangedListener(TextEditorWatcher);
    }
    private ArrayList<String> FindMatchingUsers(String prefix, AutoCompleteTextView teamUserSearchAutoComplete, TeamData teamData) {
        String url = getResources().getString(R.string.base_url) + "/Teams/SubjectTeamlessUserSearch?token=" + AuthorizedToken + "&prefix=" + prefix + "&subjectId=" + teamData.subjectId;
        ArrayList<String> usernames = new ArrayList<String>();
        ArrayList<String> ids = new ArrayList<String>();
        if (prefix.length() < 1) return usernames;

        RequestQueue queue = Volley.newRequestQueue(this);
        JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        for (int i = 0; i < response.length(); i++) {
                            try {
                                usernames.add(response.getJSONObject(i).getString("displayName"));
                                ids.add(response.getJSONObject(i).getString("id"));
                            } catch (JSONException e) {
                                e.printStackTrace();
                            }
                        }

                        ConversationsActivity.UserAdapter adapter = new ConversationsActivity.UserAdapter(getApplicationContext(), android.R.layout.select_dialog_item, usernames, ids);
                        teamUserSearchAutoComplete.setThreshold(1);
                        teamUserSearchAutoComplete.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                            @Override
                            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                                teamData.selectedUserId = adapter.ids.get(position);
                            }
                        });
                        teamUserSearchAutoComplete.setAdapter(adapter);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.teamsLinearLayout), getString(R.string.user_search_error), true);
            }
        });

        queue.add(jsonArrayRequest);
        return usernames;
    }
}
package com.app.UsosFix;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.AutoCompleteTextView;
import android.widget.ListView;
import android.widget.TextView;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonArrayRequest;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.Volley;
import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.microsoft.signalr.HubConnectionState;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.Locale;

import static java.lang.Character.isDigit;

//klasa połączona z widokiem wszystkich konwersacji (jak messenger)
public class ConversationsActivity extends BaseActivity {
    private HubConnection hubConnection;
    private ConversationAdapter adapter;
    private String selectedUserId;
    private Activity context;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_conversations);
        selectedUserId = "";
        context = this;

        String url = getResources().getString(R.string.base_url) + "/chat?token=".concat(AuthorizedToken);
        hubConnection = HubConnectionBuilder.create(url).build();

        if (hubConnection.getConnectionState() == HubConnectionState.DISCONNECTED)
            hubConnection.start();

        hubConnection.on("Receive", (message) -> {
            JsonObject messageObject = new Gson().toJsonTree(message).getAsJsonObject();
            context.runOnUiThread(() -> UpdateConversationsView(messageObject));
        }, Object.class);

        GetUserInformation(new VolleyCallback() {
            @Override
            public void onSuccess() throws JSONException {

            }
        });
        SetTextWatcher();
    }

    private void UpdateConversationsView(JsonObject message) {
        int incomingConversationId = (int)Double.parseDouble(message.get("conversationId").toString());
        String content = message.getAsJsonObject("content").get("pl").toString();
        content = content.substring(1, content.length() - 1);
        String[] dateAndTime = message.get("sentAt").toString().substring(1, 17).split("T"); //[date, time]

        ArrayList<String> namesList = adapter.conversationNames;
        ArrayList<Integer> idsList = adapter.conversationIds;
        ArrayList<String> lastMessageList = adapter.lastMessages;
        ArrayList<String> sentAtList = adapter.sentAts;

        int index = idsList.indexOf(incomingConversationId);
        namesList.add(0, namesList.get(index));
        namesList.remove(index + 1);
        idsList.add(0, idsList.get(index));
        idsList.remove(index + 1);
        lastMessageList.add(0, content);
        lastMessageList.remove(index + 1);
        sentAtList.add(0, dateAndTime[1]);
        sentAtList.remove(index + 1);

        adapter.setConversationNames(namesList);
        adapter.setConversationIds(idsList);
        adapter.setLastMessages(lastMessageList);
        adapter.setSentAts(sentAtList);

        adapter.notifyDataSetChanged();
    }

    // funkcja wywoływana w odpowiedzi na pobranie informacji o użytkowniku
    @Override
    protected void GetUserInformation_onResponseFun()
    {
        GetConversations();
    }
    // pobiera informacje o konwersacjach występujących po podanej dacie
    private void GetConversations() {
        String url = getResources().getString(R.string.base_url) + "/Messages/ConversationsSince?token=" + AuthorizedToken;//.concat("&date=2021-01-01");

        RequestQueue queue = Volley.newRequestQueue(this);
        JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        try {
                            DisplayConversations(response);
                        } catch (JSONException | ParseException e) {
                            ShowPopup(context, findViewById(R.id.conversationLinearLayout), getString(R.string.conversations_display_error), true);
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.conversationLinearLayout), getString(R.string.conversations_error), true);
            }
        });

        queue.add(jsonArrayRequest);
    }
    private void DisplayConversations(JSONArray conversations) throws JSONException, ParseException {
        String currentLang = getResources().getConfiguration().locale.getLanguage();

        ListView conversationList = findViewById(R.id.conversationsListView);
        ArrayList<String> namesList = new ArrayList<>();
        ArrayList<Integer> idsList = new ArrayList<>();
        ArrayList<String> lastMessageList = new ArrayList<>();
        ArrayList<String> sentAtList = new ArrayList<>();

        for (int i = 0; i < conversations.length(); i++) {
            JSONObject conversation = conversations.getJSONObject(i);
            JSONArray participants = conversation.getJSONArray("participants");
            String authorId = conversation.getJSONArray("messages").getJSONObject(0).getString("authorId");
            String lastMessageContent = "";
            String conversationName = "";
            for (int j = 0; j < participants.length(); j++) {
                JSONObject participant = participants.getJSONObject(j);
                String messageUserId = participant.getJSONObject("user").getString("id");
                if (UserInfo != null && !messageUserId.equals(UserInfo.getString("id"))) {
                    conversationName = conversationName.concat(participant.getJSONObject("user").getString("displayName"));
                    conversationName = conversationName.concat(", ");
                }
                else if (UserInfo != null && messageUserId.equals(authorId)) lastMessageContent = getString(R.string.you).concat(": ");
            }
            conversationName = conversationName.substring(0, conversationName.lastIndexOf(','));
            namesList.add(conversationName);
            String conversationId = conversation.getString("id");
            idsList.add(Integer.parseInt(conversationId));
            lastMessageList.add(lastMessageContent.concat(conversation.getJSONArray("messages").getJSONObject(0).getJSONObject("content").getString(currentLang)));
            sentAtList.add(ConvertDate(conversation.getJSONArray("messages").getJSONObject(0).getString("sentAt")));
        }
        adapter = new ConversationAdapter(context, namesList, idsList, lastMessageList, sentAtList);
        conversationList.setAdapter(adapter);

        conversationList.setOnItemClickListener((parent, view, position, id) -> {
            if (hubConnection.getConnectionState() == HubConnectionState.CONNECTED)
                hubConnection.stop();

            Intent MessengerIntent = new Intent(getApplicationContext(), MessengerActivity.class);
            MessengerIntent.putExtra("AuthorizedToken", AuthorizedToken);
            MessengerIntent.putExtra("ConversationId", idsList.get(position).toString());
            startActivity(MessengerIntent);
        });
    }

    private String ConvertDate(String sentAt) throws ParseException {
        String today = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).format(new Date());
        if (sentAt.substring(0, sentAt.indexOf("T")).equals(today)) { //wiadomosc wyslana dzisiaj
            return sentAt.substring(sentAt.indexOf("T") + 1, sentAt.lastIndexOf(":"));
        }
        return sentAt.substring(0, sentAt.indexOf("T"));
    }
    public void InviteToChat(View view) {

        AutoCompleteTextView userSearchAutoComplete = findViewById(R.id.searchAutoComplete);
        String toInvite = userSearchAutoComplete.getText().toString();
        if (toInvite.length() < 1)
            ShowPopup(context, findViewById(R.id.conversationLinearLayout), getString(R.string.too_short_username), true);

        if (toInvite.length() == 6) {
            int i = 0;
            boolean isIndex = true;
            while (i < toInvite.length() && isIndex) {
                if (!isDigit(toInvite.charAt(i)))
                    isIndex = false;
                i++;
            }
            //zaproś po indeksie
            //jeśli błąd, zaproś po usernamie
            if (isIndex)
                InviteByIndexRequest(toInvite);
            else
                InviteByIdRequest();
        }
        else
            InviteByIdRequest(); //zaproś po usernamie

    }
    private void InviteByIndexRequest(String toInvite) {
        String url = getResources().getString(R.string.base_url) + "/Messages/InviteToChat?token=" + AuthorizedToken + "&userToInvite=" + toInvite;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        //refresh the view
                        ListView conversationList = findViewById(R.id.conversationsListView);
                        conversationList.removeAllViews();
                        GetConversations();
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.conversationLinearLayout), getString(R.string.invite_to_chat_error), true);
            }
        });

        queue.add(jsonObjectRequest);
    }
    private void InviteByIdRequest() {
        String url = getResources().getString(R.string.base_url) + "/Messages/InviteToChatById?token=" + AuthorizedToken + "&userToInviteId=" + selectedUserId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.POST, url, null,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        ListView conversationList = findViewById(R.id.conversationsListView);
                        conversationList.removeAllViewsInLayout();
                        GetConversations();
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.conversationLinearLayout), getString(R.string.invite_to_chat_error), true);
            }
        });

        queue.add(jsonObjectRequest);
    }
    private void SetTextWatcher() {
        AutoCompleteTextView userSearchAutoComplete = findViewById(R.id.searchAutoComplete);
        final TextWatcher TextEditorWatcher = new TextWatcher() {
            public void beforeTextChanged(CharSequence s, int start, int count, int after) { }

            public void onTextChanged(CharSequence s, int start, int before, int count) {
                ArrayList<String> usernames = FindMatchingUsernames(s.toString());
            }

            public void afterTextChanged(Editable s) { }
        };
        userSearchAutoComplete.addTextChangedListener(TextEditorWatcher);
    }
    private ArrayList<String> FindMatchingUsernames(String prefix) {
        String url = getResources().getString(R.string.base_url) + "/Account/UserSearch?token=" + AuthorizedToken + "&prefix=" + prefix;
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

                        UserAdapter adapter = new UserAdapter(getApplicationContext(), android.R.layout.select_dialog_item, usernames, ids);
                        AutoCompleteTextView searchAutoComplete = findViewById(R.id.searchAutoComplete);
                        searchAutoComplete.setThreshold(1);
                        searchAutoComplete.setOnItemClickListener(new AdapterView.OnItemClickListener() {
                            @Override
                            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                                selectedUserId = ids.get(position);
                            }
                        });
                        searchAutoComplete.setAdapter(adapter);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.conversationLinearLayout), getString(R.string.user_search_error), true);
            }
        });

        queue.add(jsonArrayRequest);
        return usernames;
    }

    class ConversationAdapter extends ArrayAdapter<String> {
        Context context;
        ArrayList<String> conversationNames;
        ArrayList<Integer> conversationIds;
        ArrayList<String> lastMessages;
        ArrayList<String> sentAts;

        ConversationAdapter(Context c, ArrayList<String> names, ArrayList<Integer> ids, ArrayList<String> lasts, ArrayList<String> dates) {
            super(c, R.layout.conversation_element, R.id.conversationNameTextView, names);
            context = c;
            conversationNames = names;
            conversationIds = ids;
            lastMessages = lasts;
            sentAts = dates;
        }
        @NonNull
        @Override
        public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
            LayoutInflater inflater = (LayoutInflater)getApplicationContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            View conversationElement = inflater.inflate(R.layout.conversation_element, parent, false);

            TextView conversationName = conversationElement.findViewById(R.id.conversationNameTextView);
            conversationName.setText(conversationNames.get(position));


            TextView lastMessage = conversationElement.findViewById(R.id.lastMessageTextView);
            lastMessage.setText(lastMessages.get(position));

            TextView sentAt = conversationElement.findViewById(R.id.sentAtTextView);
            sentAt.setText(sentAts.get(position));

            return conversationElement;
        }

        public void setConversationIds(ArrayList<Integer> conversationIds) {
            this.conversationIds = conversationIds;
        }

        public void setConversationNames(ArrayList<String> conversationNames) {
            this.conversationNames = conversationNames;
        }

        public void setLastMessages(ArrayList<String> lastMessages) {
            this.lastMessages = lastMessages;
        }

        public void setSentAts(ArrayList<String> sentAts) {
            this.sentAts = sentAts;
        }
    }
    static class UserAdapter extends ArrayAdapter<String> {
        Context context;
        ArrayList<String> usernames;
        ArrayList<String> ids;

        public UserAdapter(@NonNull Context _context, int resource, @NonNull ArrayList<String> _usernames, @NonNull ArrayList<String> _ids) {
            super(_context, resource, _usernames);
            context = _context;
            usernames = _usernames;
            ids = _ids;
        }
    }
}
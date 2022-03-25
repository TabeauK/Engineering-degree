package com.app.UsosFix;

import android.app.Activity;
import android.content.Context;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Build;
import android.os.Bundle;
import android.util.Pair;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.TableLayout;
import android.widget.TextView;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonArrayRequest;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;

public class ExchangeActivity extends BaseActivity {

    String currentLang;
    private JSONArray Exchanges;
    private ArrayList<Pair<String, ArrayList<String>>> relationsArray; //<exchange1Id, exchange2Id>
    private Activity context;
    private ArrayList<String> chosenExchangeIds;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        relationsArray = new ArrayList<>();
        context = this;
        chosenExchangeIds = new ArrayList<>();

        GetUserInformation(new VolleyCallback() {
            @Override
            public void onSuccess() {
                GetExchanges();
            }
        });
        setContentView(R.layout.activity_exchange);
    }

    private void GetExchanges() {
        String url = getResources().getString(R.string.base_url) + "/Exchanges/Exchanges?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        Exchanges = response;
                        GetRelations();
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.exchangesLinearLayout), getString(R.string.exchanges_error), true);
            }
        });

        queue.add(jsonArrayRequest);
    }

    private void GetRelations() {
        String url = getResources().getString(R.string.base_url) + "/Exchanges/Relations?token=" + AuthorizedToken;
        RequestQueue queue = Volley.newRequestQueue(this);

        final JsonArrayRequest jsonArrayRequest = new JsonArrayRequest(Request.Method.GET, url, null,
                new Response.Listener<JSONArray>() {
                    @Override
                    public void onResponse(JSONArray response) {
                        try {
                            DisplayExchanges(response);
                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.exchangesLinearLayout), getString(R.string.exchanges_error), true);
            }
        });

        queue.add(jsonArrayRequest);
    }

    private void DisplayExchanges(JSONArray relations) throws JSONException {
        LinearLayout exchangesScrollableLinearLayout = findViewById(R.id.exchangesScrollableLinearLayout);
        exchangesScrollableLinearLayout.removeAllViews();
        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
        params.setMargins(0, 10, 0, 20);
        relationsArray = new ArrayList<>();

        currentLang = getResources().getConfiguration().locale.getLanguage();

        for(int i = 0; i < Exchanges.length(); i++) {
            final JSONObject exchange = Exchanges.getJSONObject(i);
            String state = exchange.getString("exchangeState");
            if (state.equals("Submitted"))
                DisplaySingleExchange(exchange, params, exchangesScrollableLinearLayout, relations);
        }
    }
    private void DisplaySingleExchange(JSONObject exchange, LinearLayout.LayoutParams params, LinearLayout exchangesScrollableLinearLayout, JSONArray relations) throws JSONException {
        final LinearLayout ExchangeElementView = (LinearLayout)getLayoutInflater().inflate(R.layout.exchange_element, null);
        ExchangeElementView.setLayoutParams(params);

        TextView subjectName = ExchangeElementView.findViewById(R.id.exchange_subjectName);
        subjectName.setText(exchange.getJSONObject("groupFrom").getJSONObject("displayName").getString(currentLang));

        TextView classType = ExchangeElementView.findViewById(R.id.exchange_classType);
        classType.setText(exchange.getJSONObject("groupFrom").getString("classType"));

        String group = getString(R.string.group);
        TextView groupFromNumber = ExchangeElementView.findViewById(R.id.groupFromInfoTextView);
        groupFromNumber.setText(group.concat(" ").concat(exchange.getJSONObject("groupFrom").getString("groupNumber")));

        TextView groupToNumber = ExchangeElementView.findViewById(R.id.groupToInfoTextView);
        groupToNumber.setText(group.concat(" ").concat(exchange.getJSONObject("groupTo").getString("groupNumber")));

        String exchangeId = exchange.getString("id");
        SetRelationButtonsOnClickListeners(ExchangeElementView, exchangeId);

        exchangesScrollableLinearLayout.addView(ExchangeElementView);

        // RELACJE
        DisplayRelationsForSingleExchange(relations, exchangeId, ExchangeElementView);
    }

    private void SetRelationButtonsOnClickListeners(View exchangeElementView, String exchangeId) {
        Button dependencyButton = exchangeElementView.findViewById(R.id.addDependencyRelationButton);
        Button excludingButton = exchangeElementView.findViewById(R.id.addExcludingRelationButton);

        dependencyButton.setOnClickListener(v -> {
            //okno do wyboru wymiany do relacji
            ShowAddRelationPopUp(exchangeId, "And");
        });

        excludingButton.setOnClickListener(v -> {
            //okno do wyboru wymiany do relacji
            ShowAddRelationPopUp(exchangeId, "Xor");
        });
    }

    private void ShowAddRelationPopUp(String exchangeId, String relationType) {
        LayoutInflater layoutInflater = (LayoutInflater) getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View popUpLayout = layoutInflater.inflate(R.layout.pop_up_add_relation, null);
        LinearLayout exchangesLinearLayout = findViewById(R.id.exchangesLinearLayout);

        PopupWindow popUp = new PopupWindow(context);
        popUp.setContentView(popUpLayout);
        popUp.setWidth(LinearLayout.LayoutParams.WRAP_CONTENT);
        popUp.setHeight(LinearLayout.LayoutParams.WRAP_CONTENT);
        popUp.setFocusable(false);
        popUp.setOutsideTouchable(false);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) { ;
            exchangesLinearLayout.setForeground(new ColorDrawable(0xCC000000));
        }

        popUp.setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        popUp.showAtLocation(exchangesLinearLayout, Gravity.CENTER, 0, 0);

        try {
            DisplayExchangesToChoose(Exchanges, popUpLayout, exchangeId);
        } catch (JSONException e) {
            e.printStackTrace();
        }

        Button addRelationsButton = popUpLayout.findViewById(R.id.addRelationsButton);
        addRelationsButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                popUp.dismiss();
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                    exchangesLinearLayout.setForeground(new ColorDrawable(0x00));
                }
                AddRelations(exchangeId, relationType);
            }
        });

        Button cancelButton = popUpLayout.findViewById(R.id.addRelations_cancelButton);
        cancelButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                popUp.dismiss();
                chosenExchangeIds = new ArrayList<>();
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                    exchangesLinearLayout.setForeground(new ColorDrawable(0x00));
                }
            }
        });
    }

    private void DisplayRelationsForSingleExchange(JSONArray relations, String exchangeId, View exchangeElementView) throws JSONException {

        ArrayList<String> relatedExchanges = new ArrayList<>();

        for (int j = 0; j < relations.length(); j++) {
            JSONObject relation = relations.getJSONObject(j);
            JSONArray exchangesInRelation = relation.getJSONArray("exchanges");
            String relationId = relation.getString("id");

            for (int k = 0; k < exchangesInRelation.length(); k++) { //k={0,1}
                if (exchangeId.equals(exchangesInRelation.getJSONObject(k).getString("id"))) { //jeśli id wymiany w relacji jest aktualną relacją
                    int modK = (k + 1) % 2;
                    relatedExchanges.add(exchangesInRelation.getJSONObject(modK).getString("id"));
                    TableLayout exchangeElementTable = exchangeElementView.findViewById(R.id.exchangeElementTable);
                    exchangeElementTable.setBackgroundColor(getResources().getColor(R.color.colorWhite));
                    if (relation.getString("type").equals("And")) {
                        TextView inBidingRelationWithTextView = exchangeElementView.findViewById(R.id.inBindingRelationWithTextView);
                        inBidingRelationWithTextView.setVisibility(View.VISIBLE);
                        // dodać widok wymiany
                        AddRelatedExchangeView(exchangeElementView.findViewById(R.id.bindingRelationsTable), exchangesInRelation.getJSONObject(modK), relationId);
                    }
                    else if (relation.getString("type").equals("Xor")) {
                        TextView inExclusionRelationWithTextView = exchangeElementView.findViewById(R.id.inExclusionRelationWithTextView);
                        inExclusionRelationWithTextView.setVisibility(View.VISIBLE);
                        //dodać widok wymiany
                        exchangeElementView.findViewById(R.id.bindingRelationsTable).setBackgroundColor(getResources().getColor(R.color.colorWhite));
                        AddRelatedExchangeView(exchangeElementView.findViewById(R.id.exclusionRelationsTable), exchangesInRelation.getJSONObject(modK), relationId);
                    }
                }
            }
        }

        if (relatedExchanges.size() > 0)
            relationsArray.add(new Pair(exchangeId, relatedExchanges));
    }

    private void AddRelatedExchangeView(TableLayout exchangeElementTable, JSONObject exchange, String relationId) throws JSONException {
        LinearLayout exchangeInfo = (LinearLayout) getLayoutInflater().inflate(R.layout.related_exchange_info, null);

        TextView subjectName = exchangeInfo.findViewById(R.id.subjectNameTextView);
        subjectName.setText(exchange.getJSONObject("groupFrom").getJSONObject("displayName").getString(currentLang));

        String group = getString(R.string.group);
        TextView groupFromNumber = exchangeInfo.findViewById(R.id.groupFromInfoTextView);
        groupFromNumber.setText(group.concat(" ").concat(exchange.getJSONObject("groupFrom").getString("groupNumber")));

        TextView groupToNumber = exchangeInfo.findViewById(R.id.groupToInfoTextView);
        groupToNumber.setText(group.concat(" ").concat(exchange.getJSONObject("groupTo").getString("groupNumber")));

        Button removeRelationButton = exchangeInfo.findViewById(R.id.removeRelationButton);
        removeRelationButton.setOnClickListener(v -> RemoveRelation(relationId));

        exchangeElementTable.addView(exchangeInfo);
    }

    private void RemoveRelation(String relationId) {
        String url = getResources().getString(R.string.base_url) + "/Exchanges/DeleteRelation?token=" + AuthorizedToken + "&relationId=" + relationId;
        RequestQueue queue = Volley.newRequestQueue(this);

        final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        GetRelations();
                        ShowPopup(context, findViewById(R.id.exchangesLinearLayout), getString(R.string.cancel_relation_success), false);
                    }
                }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                ShowPopup(context, findViewById(R.id.exchangesLinearLayout), getString(R.string.cancel_relation_error), true);
            }
        });

        queue.add(stringRequest);
    }
    private void DisplayExchangesToChoose(JSONArray response, View popUpLayout, String exchangeId) throws JSONException {
        LinearLayout exchangesToChooseScrollableLinearLayout = popUpLayout.findViewById(R.id.exchangesToChooseScrollableLinearLayout);

        ArrayList<String> relatedExchanges = new ArrayList<>();
        for (int i = 0; i < relationsArray.size(); i++) {
                if (relationsArray.get(i).first.equals(exchangeId))
                    relatedExchanges.addAll(relationsArray.get(i).second);
            }

        for (int i = 0; i < response.length(); i++) {
            final JSONObject exchange = response.getJSONObject(i);
            if (!exchange.getString("id").equals(exchangeId)
                    && !relatedExchanges.contains(exchange.getString("id"))) {
                final LinearLayout ExchangeElementView = (LinearLayout)getLayoutInflater().inflate(R.layout.exchange_element_to_choose, null);
                LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
                params.setMargins(0, 0, 0, 10);
                ExchangeElementView.setLayoutParams(params);

                TextView exchangeSubjectName = ExchangeElementView.findViewById(R.id.exchangeToChoose_subjectName);
                exchangeSubjectName.setText(exchange.getJSONObject("groupFrom").getJSONObject("displayName").getString(currentLang));
                ExchangeElementView.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        String clickedId = "";
                        try {
                            clickedId = exchange.getString("id");
                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                        if (clickedId.length() > 0) { //layout_marginHorizontal
                            if (chosenExchangeIds.contains(clickedId)) {
                                chosenExchangeIds.remove(clickedId);
                                v.setBackground(getResources().getDrawable(R.drawable.exchange_to_choose_background));
                            }
                            else {
                                chosenExchangeIds.add(clickedId);
                                v.setBackground(getResources().getDrawable(R.drawable.chosen_exchange_background));
                            }
                        }
                    }
                });

                TextView classType = ExchangeElementView.findViewById(R.id.exchangeToChoose_classType);
                classType.setText(exchange.getJSONObject("groupFrom").getString("classType"));

                String group = getString(R.string.group);
                TextView groupFromNumber = ExchangeElementView.findViewById(R.id.groupFromInfoTextView);
                groupFromNumber.setText(group.concat(" ").concat(exchange.getJSONObject("groupFrom").getString("groupNumber")));

                TextView groupToNumber = ExchangeElementView.findViewById(R.id.groupToInfoTextView);
                groupToNumber.setText(group.concat(" ").concat(exchange.getJSONObject("groupTo").getString("groupNumber")));

                exchangesToChooseScrollableLinearLayout.addView(ExchangeElementView, 0);
            }
        }
    }
    public void AddRelations(String exchangeId, String relationType) {
        String url;
        final boolean[] isSuccess = {true};
        RequestQueue queue = Volley.newRequestQueue(this);
        for (int i = 0; i < chosenExchangeIds.size(); i++) {
            url = "https://usos-fix.herokuapp.com/Exchanges/AddRelation?token=" + AuthorizedToken + "&exchange1Id=" + exchangeId + "&exchange2Id=" + chosenExchangeIds.get(i) + "&relationType=" + relationType;

            final StringRequest stringRequest = new StringRequest(Request.Method.PUT, url,
                    new Response.Listener<String>() {
                        @Override
                        public void onResponse(String response) {
                            //odświeżenie listy wymian
                            GetRelations();
                        }
                    }, new Response.ErrorListener() {
                @Override
                public void onErrorResponse(VolleyError error) {
                    isSuccess[0] = false;
                    ShowPopup(context, findViewById(R.id.exchangesLinearLayout), getString(R.string.add_relation_error), true);
                }
            });
            if (isSuccess[0])
                ShowPopup(context, findViewById(R.id.exchangesLinearLayout), getString(R.string.add_relation_success), false);

            queue.add(stringRequest);
        }
    }
}